using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace DiceRollerUtils
{
    public class DiceRoller
    {
        private readonly IRandomNumberGenerator numberGenerator;
        private SortedDictionary<int, List<string>> diceBucket;

        public DiceRoller(IRandomNumberGenerator numberGenerator = null)
        {
            this.numberGenerator = numberGenerator ?? new RandomNumberGenerator();
            diceBucket = new SortedDictionary<int, List<string>>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
        }

        public int CalculateRoll(string fullRoll, out string resultString)
        {
            const string pattern = @"([+|-]?\s?\d*\/?d\d+)|([+|-]?\s?\d+)";
            var regExp = new Regex(pattern, RegexOptions.IgnoreCase);

            int totalRoll = 0;

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
                        expression = $@"{posneg}{RollDice(sides)}";

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

            var fullDescription = "Breakdown:\n";

            foreach (var key in diceBucket.Keys)
            {
                var diceLabel = key == 0 ? "modifiers:" : $"{diceBucket[key].Count}d{key}:";
                    
                fullDescription += $"  {diceLabel} {String.Join(" ", diceBucket[key].ToArray())}\n";
            }

            resultString = fullDescription;

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

        private int RollDice(int sides)
        {
            return this.numberGenerator.Generate(1, sides + 1);
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
