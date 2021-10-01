using System;

namespace DiceRollerUtils
{
    public interface IRandomNumberGenerator
    {
        int Generate(int minValue, int maxValue);
    }

    internal class RandomNumberGenerator : IRandomNumberGenerator
    {
        private static readonly Random Random = new Random();

        public int Generate(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }
    }
}
