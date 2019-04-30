using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace DiceRollerUtils
{
    public enum RollType
    {
        normalRoll,
        withAdvantage,
        withDisadvantage
    };

    public class DiceRoller
    {
        private readonly IRandomNumberGenerator numberGenerator;
        private SortedDictionary<int, List<string>> diceBucket;
        private List<int> throwAwayRolls = new List<int>();

        public DiceRoller(IRandomNumberGenerator numberGenerator = null)
        {
            this.numberGenerator = numberGenerator ?? new RandomNumberGenerator();
            diceBucket = new SortedDictionary<int, List<string>>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
        }

        public RollType GetRollType(string fullRoll)
        {
            var match = Regex.Match(fullRoll, @"(?<advantage>\s+\/?adv)|(?<disadvantage>\s+\/?dis)", RegexOptions.IgnoreCase);
            if (match.Length == 0)
            {
                return RollType.normalRoll;
            }

            return (match.Groups["disadvantage"].Length > 0) ? RollType.withDisadvantage : RollType.withAdvantage;
        }

        public int CalculateRoll(string fullRoll, out string resultString)
        {
            var rollType = GetRollType(fullRoll);
            int totalRoll = 0;
            const string pattern = @"([+|-]?\s?\d*\/?d\d+)|([+|-]?\s?\d+)";

            var regExp = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Match m in regExp.Matches(fullRoll))
            {
                string expression;

                var diceMatch = Regex.Match(m.Value, @"(?<posneg>[+|-]?)\s?\/?(?<multiplier>[\d]+)?d(?<sides>\d+)", RegexOptions.IgnoreCase);
                if (diceMatch.Length > 0)
                {
                    string posneg = diceMatch.Groups["posneg"].Value == "-" ? "-" : "+";
                    int multiplier = String.IsNullOrWhiteSpace(diceMatch.Groups["multiplier"].Value) ? 1 : Int32.Parse(diceMatch.Groups["multiplier"].Value);
                    int sides = Int32.Parse(diceMatch.Groups["sides"].Value);

                    for (var i = 0; i < multiplier; i++)
                    {
                        expression = $@"{posneg}{RollDice(sides, rollType)}";

                        int rollValue = ExpressionToInt(expression);

                        totalRoll += rollValue;
                        AddToDiceBucket(sides, IntToExpression(rollValue));
                    }
                }
                else
                {
                    expression = m.Value;
                    var rollValue = ExpressionToInt(expression);

                    totalRoll += ExpressionToInt(expression);
                    AddToDiceBucket(0, IntToExpression(rollValue));
                }
            }

            var fullDescription = new List<string>(); 

            foreach (var key in diceBucket.Keys)
            {
                var diceLabel = key == 0 ? "Mod:" : $"{diceBucket[key].Count}d{key}:";

                fullDescription.Add($"{diceLabel} (*{String.Join(", ", diceBucket[key].ToArray())}*)");
            }

            // Output the rolls that were tossed out because of advantage/disadvantage.
            if (throwAwayRolls.Count > 0)
            {
                fullDescription.Add($"Thrown out: (*{String.Join(", ", throwAwayRolls.ToArray())}*)\n");
            }

            resultString = $"*{totalRoll}*  : " + String.Join(", ", fullDescription.ToArray());

            return totalRoll;
        }

        private void AddToDiceBucket(int sides, string expression)
        {
            if (diceBucket.TryGetValue(sides, out List<string> rolls))
            {
                rolls.Add(expression);
            }
            else
            {
                diceBucket.Add(sides, new List<string> { expression });
            }
        }

        private int RollDice(int sides, RollType rollType)
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
            var dt = new DataTable();
            return (int)dt.Compute(expression, "");
        }
    }
}
