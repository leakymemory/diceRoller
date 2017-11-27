using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace DiceRollerUtils
{
    public class DiceRoller
    {
        private readonly IRandomNumberGenerator numberGenerator;

        public DiceRoller(IRandomNumberGenerator numberGenerator = null)
        {
            this.numberGenerator = numberGenerator ?? new RandomNumberGenerator();
        }

        public string CalculateRoll(string fullRoll)
        {
            const string pattern = @"([+|-]?\d*\/?d\d+)|([+|-]?\d+)";
            var regExp = new Regex(pattern, RegexOptions.IgnoreCase);

            var rollDescription = new List<string>();
            int totalRoll = 0;

            foreach (Match m in regExp.Matches(fullRoll))
            {
                string expression;
                bool isDiceRoll = false;

                var diceMatch = Regex.Match(m.Value, @"(?<posneg>[+|-]?)\/?(?<multiplier>[\d]+)?d(?<sides>\d+)", RegexOptions.IgnoreCase);
                if (diceMatch.Length > 0)
                {
                    isDiceRoll = true;
                    string posneg = diceMatch.Groups["posneg"].Value == "-" ? "-" : "+";
                    int multiplier = String.IsNullOrWhiteSpace(diceMatch.Groups["multiplier"].Value) ? 1 : Int32.Parse(diceMatch.Groups["multiplier"].Value);
                    int sides = Int32.Parse(diceMatch.Groups["sides"].Value);

                    for (var i = 0; i < multiplier; i++)
                    {
                        expression = $@"{posneg}{RollDice(sides)}";

                        int rollValue = ExpressionToInt(expression);
                        rollDescription.Add(IntToExpression(rollValue, isDiceRoll));

                        totalRoll += rollValue;
                    }
                }
                else
                {
                    expression = m.Value;

                    int rollValue = ExpressionToInt(expression);
                    rollDescription.Add(IntToExpression(rollValue, isDiceRoll));

                    totalRoll += rollValue;
                }
            }
            rollDescription.Add($"= {totalRoll}");

            return String.Join(" ", rollDescription.ToArray());
        }

        private int RollDice(int sides)
        {
            return this.numberGenerator.Generate(1, sides + 1);
        }

        private static string IntToExpression(int value, bool isDiceRoll)
        {
            if (isDiceRoll)
            {
                return value > 0 ? $"(+{value})" : $"({value})";
            }

            return value > 0 ? $"+{value}" : $"{value}";
        }

        private static int ExpressionToInt(string expression)
        {
            var dt = new DataTable();
            return (int)dt.Compute(expression, "");
        }
    }
}
