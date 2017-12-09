using NUnit.Framework;

using DiceRollerUtils;

namespace DiceRollerTests
{
    [TestFixture()]
    public class DiceRollerTests
    {
        private readonly MockRandomNumberGenerator numberGenerator = new MockRandomNumberGenerator();

        [Test()]
        public void CalculateRoll_ComplicatedRoll()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            string result;
            int total = diceRoller.CalculateRoll(@"/d20 + 5 -3-/d10 + 7-2", out result);

            Assert.AreEqual(14, total);
            Assert.AreEqual("Breakdown:\n  1d20: +10\n  1d10: -3\n  modifiers: +5 -3 +7 -2\n", result);

            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(2);

            total = diceRoller.CalculateRoll(@"d20 -/d6-2+3+/d4", out result);
            Assert.AreEqual(10, total);
            Assert.AreEqual("(+10) (-3) -2 +3 (+2) = 10", result);
        }

        [Test()]
        public void CalculateRoll_DiceMultiplier()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(5);
            numberGenerator.QueuedResults.Enqueue(8);
            numberGenerator.QueuedResults.Enqueue(3);

            string result;
            int total = diceRoller.CalculateRoll(@"2d20 + 5 - 3d6", out result);

            Assert.AreEqual(14, total);
            Assert.AreEqual("Breakdown:\n  2d20: +10 +15\n  3d6: -5 -8 -3\n  modifiers: +5\n", result);
        }

        [Test()]
        public void CalculateRoll_LargeDiceMultiplier()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(15);

            string result;
            int total = diceRoller.CalculateRoll(@"12d20 + 5", out result);

            Assert.AreEqual(185, total);
            Assert.AreEqual("(+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) +5 = 185", result);
        }

        [Test()]
        public void CalculateRoll_ComplicatedRollWithBadData()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(5);

            string result;
            int total = diceRoller.CalculateRoll(@"d20 + fred -george !r#andom %%text IS $here + 5-3-2d10 + 7-2", out result);

            Assert.AreEqual(9, total);
            Assert.AreEqual("(+10) +5 -3 (-3) (-5) +7 -2 = 9", result);
        }

        [Test()]
        public void CalculateRoll_RollWithUserSpecified()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            string result;
            int total = diceRoller.CalculateRoll(@"1d20 +4 +/d6 @fred", out result);

            Assert.AreEqual(17, total);
            Assert.AreEqual("(+10) +4 (+3) = 17", result);
        }

        [Test()]
        public void CalculateRoll_RollWithAdvantage()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            string result;
            int total = diceRoller.CalculateRoll(@"d20+4 /adv", out result);

            Assert.AreEqual(14, total);
            Assert.AreEqual("(+10) +4 = 14", result);
        }

        [Test()]
        public void CalculateRoll_RollWithDisadvantage()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            string result;
            int total = diceRoller.CalculateRoll(@"d20+4 /dis", out result);

            Assert.AreEqual(7, total);
            Assert.AreEqual("(+3) +4 = 7", result);
        }

        [Test()]
        public void CalculateRoll_RollWithAdvantageAndDisadvantage()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);

            string result;
            int total = diceRoller.CalculateRoll(@"d20+4 /dis /adv", out result);

            Assert.AreEqual(14, total);
            Assert.AreEqual("(+10) +4 = 14", result);
        }
    }
}
