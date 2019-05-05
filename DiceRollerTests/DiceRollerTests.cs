using NUnit.Framework;

using DiceRollerUtils;

namespace DiceRollerTests
{
    [TestFixture()]
    public class DiceRollerTests
    {
        private readonly MockRandomNumberGenerator numberGenerator = new MockRandomNumberGenerator();

        [Test()]
        public void CalculateRoll_MultipleLists()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            var allRolls = diceRoller.CalculateRoll("/d20 +5, d10+3");

            Assert.IsTrue(allRolls.Count == 2);
            Assert.AreEqual("*15*  : 1d20: (*+10*), Mod: (*+5*)", allRolls[0]);
            Assert.AreEqual("*6*  : 1d10: (*+3*), Mod: (*+3*)", allRolls[1]);
        }

        [Test()]
        public void CalculateRoll_ComplicatedRoll()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            var roll = diceRoller.ParseRoll(@"/d20 + 5 -3-/d10 + 7-2");
            Assert.AreEqual("*14*  : 1d20: (*+10*), 1d10: (*-3*), Mod: (*+5, -3, +7, -2*)", roll);

            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(2);

            roll = diceRoller.ParseRoll(@"d20 -/d6-2+3+/d4");
            Assert.AreEqual("*10*  : 1d20: (*+10*), 1d6: (*-3*), 1d4: (*+2*), Mod: (*-2, +3*)", roll);
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

            var roll = diceRoller.ParseRoll(@"2d20 + 5 - 3d6");

            Assert.AreEqual("*14*  : 2d20: (*+10, +15*), 3d6: (*-5, -8, -3*), Mod: (*+5*)", roll);
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

            var roll = diceRoller.ParseRoll(@"12d20 + 5");

            Assert.AreEqual("*185*  : 12d20: (*+15, +15, +15, +15, +15, +15, +15, +15, +15, +15, +15, +15*), Mod: (*+5*)", roll);
        }

        [Test()]
        public void CalculateRoll_ComplicatedRollWithBadData()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(5);

            var roll = diceRoller.ParseRoll(@"d20 + fred -george !r#andom %%text IS $here + 5-3-2d10 + 7-2");

            Assert.AreEqual("*9*  : 1d20: (*+10*), 2d10: (*-3, -5*), Mod: (*+5, -3, +7, -2*)", roll);
        }

        [Test()]
        public void CalculateRoll_RollWithUserSpecified()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            var roll = diceRoller.ParseRoll(@"1d20 +4 +/d6 @fred");

            Assert.AreEqual("*17*  : 1d20: (*+10*), 1d6: (*+3*), Mod: (*+4*)", roll);
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

            var roll = diceRoller.ParseRoll(@"2d20+4 /adv");

            Assert.AreEqual("*34*  : 2d20: (*+10, +20*), Mod: (*+4*), Thrown out: (*3, 6*)\n", roll);
        }

        [Test()]
        public void CalculateRoll_RollWithAdvantage()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            var roll = diceRoller.ParseRoll(@"d20+4 adv");

            Assert.AreEqual("*14*  : 1d20: (*+10*), Mod: (*+4*), Thrown out: (*3*)\n", roll);
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

            var roll = diceRoller.ParseRoll(@"d20 +2d8 -7 +2 -1d4 adv");

            Assert.AreEqual("*13*  : 1d20: (*+10*), 2d8: (*+4, +6*), 1d4: (*-2*), Mod: (*-7, +2*), Thrown out: (*3*)\n", roll);
        }

        [Test()]
        public void CalculateRoll_RollWithDisadvantage()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            var roll = diceRoller.ParseRoll(@"d20+4 /dis");

            Assert.AreEqual("*7*  : 1d20: (*+3*), Mod: (*+4*), Thrown out: (*10*)\n", roll);
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
