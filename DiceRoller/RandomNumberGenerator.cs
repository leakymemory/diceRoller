using System;

namespace DiceRollerUtils
{
    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        private static readonly Random Random = new Random();

        public int Generate(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }
    }
}
