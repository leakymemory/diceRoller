using System;
using System.Diagnostics;

using NUnit.Framework;

using DiceRollerUtils;

namespace DiceRollerTests
{
    [TestFixture()]
    public class RandomNumberGeneratorTests
    {
        [Test()]
        public void ValidateRandomNumberGenerator()
        {
            const int sampleSize = 10000;
            const int numberOfSides = 6;
            var buckets = new int[numberOfSides];

            var randomNumberGenerator = new RandomNumberGenerator();

            for (var i = 0; i < sampleSize; i++)
            {
                var randomNumber = randomNumberGenerator.Generate(1, numberOfSides + 1);
                buckets[randomNumber - 1]++;
            }

            double perfect = Convert.ToDouble(sampleSize) / Convert.ToDouble(numberOfSides);
            foreach (var countInBucket in buckets)
            {
                var result = (double)countInBucket / perfect;
                Debug.WriteLine("Count: " + countInBucket + " : " + (100 - Math.Abs(result - 1)));
                Assert.IsTrue((100 - Math.Abs(result - 1)) >= 99.9);
            }
        }
    }
}
