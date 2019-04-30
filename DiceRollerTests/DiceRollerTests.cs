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
            Assert.AreEqual("*14*  : 1d20: (*+10*), 1d10: (*-3*), Mod: (*+5, -3, +7, -2*)", result);

            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(2);

            total = diceRoller.CalculateRoll(@"d20 -/d6-2+3+/d4", out result);
            Assert.AreEqual(10, total);
            Assert.AreEqual("*10*  : 2d20: (*+10, +10*), 1d10: (*-3*), 1d6: (*-3*), 1d4: (*+2*), Mod: (*+5, -3, +7, -2, -2, +3*)", result);
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
            Assert.AreEqual("*14*  : 2d20: (*+10, +15*), 3d6: (*-5, -8, -3*), Mod: (*+5*)", result);
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
            Assert.AreEqual("Breakdown:\n  12d20: +15 +15 +15 +15 +15 +15 +15 +15 +15 +15 +15 +15\n  modifiers: +5\n", result);
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
            Assert.AreEqual("Breakdown:\n  1d20: +10\n  2d10: -3 -5\n  modifiers: +5 -3 +7 -2\n", result);
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
            Assert.AreEqual("Breakdown:\n  1d20: +10\n  1d6: +3\n  modifiers: +4\n", result);
        }

        [Test()]
        public void CalculateRoll_RollWithAdvantageMultipleD20s()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            numberGenerator.QueuedResults.Enqueue(6);
            numberGenerator.QueuedResults.Enqueue(20);

            // Right now the way this is coded, each d20 will get multiple rolls, so effectively each roll
            // will be rolled with advantage.  Not sure if this is what we really want in the end or not.

            string result;
            int total = diceRoller.CalculateRoll(@"2d20+4 /adv", out result);

            Assert.AreEqual(34, total);
            Assert.AreEqual("Breakdown:\n  2d20: +10 +20\n  modifiers: +4\n  Thrown out: 3 6\n", result);
        }

        [Test()]
        public void CalculateRoll_RollWithAdvantage()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            string result;
            int total = diceRoller.CalculateRoll(@"d20+4 adv", out result);

            Assert.AreEqual(14, total);
            Assert.AreEqual("Breakdown:\n  1d20: +10\n  modifiers: +4\n  Thrown out: 3\n", result);
        }

        [Test()]
        public void CalculateRoll_RollWithAdvantageMoreComplicatedRoll()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(4);
            numberGenerator.QueuedResults.Enqueue(6);
            numberGenerator.QueuedResults.Enqueue(2);

            string result;
            int total = diceRoller.CalculateRoll(@"d20 +2d8 -7 +2 -1d4 adv", out result);

            Assert.AreEqual(13, total);
            Assert.AreEqual("Breakdown:\n  1d20: +10\n  2d8: +4 +6\n  1d4: -2\n  modifiers: -7 +2\n  Thrown out: 3\n", result);
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
            Assert.AreEqual("Breakdown:\n  1d20: +3\n  modifiers: +4\n  Thrown out: 10\n", result);
        }

        [Test()]
        public void GetRollType_ValidatePossibilities()
        {
            var diceRoller = new DiceRoller(numberGenerator);

            Assert.AreEqual(RollType.normalRoll, diceRoller.GetRollType("d20 + 4"));
            Assert.AreEqual(RollType.withAdvantage, diceRoller.GetRollType("d20 + 4 adv"));
            Assert.AreEqual(RollType.withAdvantage, diceRoller.GetRollType("d20 + 4 /adv"));
            Assert.AreEqual(RollType.withAdvantage, diceRoller.GetRollType("d20 + 4 with advantage"));
            Assert.AreEqual(RollType.withDisadvantage, diceRoller.GetRollType("d20 + 4 dis"));
            Assert.AreEqual(RollType.withDisadvantage, diceRoller.GetRollType("d20 + 4 /dis"));
            Assert.AreEqual(RollType.withDisadvantage, diceRoller.GetRollType("d20 + 4 with disadvantage"));
        }
    }
}
