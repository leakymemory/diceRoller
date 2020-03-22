using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("DiceRollerTests")]
namespace DiceRollerUtils
{
    public enum RollType
    {
        normalRoll,
        withAdvantage,
        withDisadvantage,
        showUsage
    };

    public class DiceRoller
    {
        private readonly IRandomNumberGenerator numberGenerator;

        private readonly string usageText = @"
/roll [label]: [diceType(s)] +/- [modifier] [adv|dis]
Examples:
    /roll Attack: d20 +5 adv
    /roll Damage: 2d10 + 1d6 +8
    /roll 12d6";

        public DiceRoller(IRandomNumberGenerator numberGenerator = null)
        {
            this.numberGenerator = numberGenerator ?? new RandomNumberGenerator();
        }

        public List<string> CalculateRoll(string fullRoll)
        {
            var allRolls = new List<string>();

            var individualRolls = fullRoll.Split(',');
            foreach (var roll in individualRolls)
            {
                allRolls.Add(ParseRoll(roll));
            }

            return allRolls;
        }

        internal RollType GetRollType(string fullRoll)
        {
            var match = Regex.Match(fullRoll, @"(?<usage>\?|help)|(?<advantage>\s+\/?adv)|(?<disadvantage>\s+\/?dis)", RegexOptions.IgnoreCase);
            if (match.Length == 0)
            {
                return RollType.normalRoll;
            }

            if (match.Groups["usage"].Length > 0)
                return RollType.showUsage;

            return (match.Groups["disadvantage"].Length > 0) ? RollType.withDisadvantage : RollType.withAdvantage;
        }

        internal string ParseRoll(string roll)
        {
            var rollType = GetRollType(roll);
            if (rollType == RollType.showUsage || roll.Equals(string.Empty))
                return usageText;

            List<int> throwAwayRolls = new List<int>();
            SortedDictionary<int, List<string>> diceBucket = BuildDiceBucket(roll, rollType, throwAwayRolls);

            var fullDescription = new List<string>();
            string criticalMessage = string.Empty;
            int totalRoll = 0;

            foreach (var key in diceBucket.Keys)
            {
                var diceLabel = key == 0 ? "Mod:" : $"{diceBucket[key].Count}d{key}:";

                fullDescription.Add($"{diceLabel} (*{String.Join(", ", diceBucket[key].ToArray())}*)");

                foreach (string value in diceBucket[key].ToArray())
                {
                    // We really only want to check for criticals when a single d20 is rolled.
                    if (key == 20 && diceBucket[key].Count == 1)
                    {
                        criticalMessage = ParseForCritical(value);
                    }

                    totalRoll += ExpressionToInt(value);
                }
            }

            // Output the rolls that were tossed out because of advantage/disadvantage.
            if (throwAwayRolls.Count > 0)
            {
                fullDescription.Add($"Thrown out: (*{String.Join(", ", throwAwayRolls.ToArray())}*)");
            }

            return $"{criticalMessage}{ParseForLabel(roll)} *{totalRoll}*  :  " + String.Join(", ", fullDescription.ToArray());
        }

        internal SortedDictionary<int, List<string>> BuildDiceBucket(string roll, RollType rollType, List<int> throwAwayRolls)
        {
            SortedDictionary<int, List<string>> diceBucket = new SortedDictionary<int, List<string>>(Comparer<int>.Create((x, y) => y.CompareTo(x)));

            const string pattern = @"([+|-]?\d*df)|([+|-]?\s?\d*\/?d\d+)|([+|-]?\s?\d+)";

            var regExp = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Match m in regExp.Matches(roll))
            {
                string expression;
                bool isFudgeDice = false;
                int sides = 0;

                var diceMatch = Regex.Match(m.Value, @"(?<posneg>[+|-]?)\s?\/?(?<multiplier>[\d]+)?d(?<sides>\d+|f)", RegexOptions.IgnoreCase);
                if (diceMatch.Length > 0)
                {
                    string posneg = diceMatch.Groups["posneg"].Value == "-" ? "-" : "+";
                    int multiplier = String.IsNullOrWhiteSpace(diceMatch.Groups["multiplier"].Value) ? 1 : Int32.Parse(diceMatch.Groups["multiplier"].Value);

                    if (diceMatch.Groups["sides"].Value.ToLower() == "f")
                    {
                        isFudgeDice = true;
                        sides = 3;
                    }
                    else
                    {
                        sides = Int32.Parse(diceMatch.Groups["sides"].Value);
                    }

                    for (var i = 0; i < multiplier; i++)
                    {
                        expression = $@"{posneg}{RollDice(sides, rollType, throwAwayRolls)}";
                        AddToDiceBucket(diceBucket, sides, expression, isFudgeDice);
                    }
                }
                else
                {
                    expression = m.Value;
                    var rollValue = ExpressionToInt(expression);

                    AddToDiceBucket(diceBucket, 0, IntToExpression(rollValue), isFudgeDice:false);
                }
            }

            return diceBucket;
        }

        internal string ParseForLabel(string roll)
        {
            var labelMatch = Regex.Match(roll, @"(?<label>[^:]+:)", RegexOptions.IgnoreCase);
            if (labelMatch.Length > 0)
            {
                return labelMatch.Groups["label"].Value;
            }

            return "Total:";
        }

        internal string ParseForCritical(string rollValue)
        {
            if (rollValue.Equals("+20"))
                return "_Critical Success!_ ";
            if (rollValue.Equals("+1"))
                return "_Critical Fail!_ ";

            return string.Empty;
        }

        private void AddToDiceBucket(SortedDictionary<int, List<string>> diceBucket, int sides, string expression, bool isFudgeDice)
        {
            string translatedExpression = expression;

            if (isFudgeDice)
            {
                switch (expression)
                {
                    case "+1":
                        translatedExpression = "-1";
                        break;
                    case "+2":
                        translatedExpression = "+0";
                        break;
                    case "+3":
                        translatedExpression = "+1";
                        break;
                }
            }

            if (diceBucket.TryGetValue(sides, out List<string> rolls))
            {
                rolls.Add(translatedExpression);
            }
            else
            {
                diceBucket.Add(sides, new List<string> { translatedExpression });
            }
        }

        private int RollDice(int sides, RollType rollType, List<int> throwAwayRolls)
        {
            int roll = this.numberGenerator.Generate(1, sides + 1);

            if (sides == 20 && rollType != RollType.normalRoll)
            {
                int secondRoll = this.numberGenerator.Generate(1, sides + 1);
                if (rollType == RollType.withAdvantage)
                {
                    throwAwayRolls.Add((secondRoll > roll) ? roll : secondRoll);
                    roll = (secondRoll > roll) ? secondRoll : roll;
                }
                else
                {
                    throwAwayRolls.Add((secondRoll < roll) ? roll : secondRoll);
                    roll = (secondRoll < roll) ? secondRoll : roll;
                }
            }

            return roll;
        }

        private static string IntToExpression(int value)
        {
            return value >= 0 ? $"+{value}" : $"{value}";
        }

        private static int ExpressionToInt(string expression)
        {
            using (var dt = new DataTable())
                return (int)dt.Compute(expression, "");
        }
    }
}
