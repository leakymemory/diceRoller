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

            string result = diceRoller.CalculateRoll(@"/d20 + 5 -3-/d10 + 7-2");
            Assert.AreEqual("(+10) +5 -3 (-3) +7 -2 = 14", result);

            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(2);
            result = diceRoller.CalculateRoll(@"d20 -/d6-2+3+/d4");
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

            string result = diceRoller.CalculateRoll(@"2d20 + 5 - 3d6");
            Assert.AreEqual("(+10) (+15) +5 (+5) (+8) (+3) = 46", result);
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

            string result = diceRoller.CalculateRoll(@"12d20 + 5");
            Assert.AreEqual("(+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) (+15) +5 = 185", result);
        }

        [Test()]
        public void CalculateRoll_ComplicatedRollWithBadData()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(5);

            string result = diceRoller.CalculateRoll(@"d20 + fred -george !r#andom %%text IS $here + 5-3-2d10 + 7-2");
            Assert.AreEqual("(+10) +5 -3 (-3) (-5) +7 -2 = 9", result);
        }

        [Test()]
        public void CalculateRoll_RollWithUserSpecified()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            string result = diceRoller.CalculateRoll(@"1d20 +4 +/d6 @fred");
            Assert.AreEqual("(+10) +4 (+3) = 17", result);
        }
    }
}
