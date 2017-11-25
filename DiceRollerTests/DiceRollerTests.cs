using NUnit.Framework;

using DiceRollerUtils;

namespace DiceRollerTests
{
    [TestFixture()]
    public class DiceRollerTests
    {
        private readonly MockRandomNumberGenerator numberGenerator = new MockRandomNumberGenerator();

        [Test()]
        public void CalculateRoll_ComplicatedRoll_Succeeds()
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
        public void CalculateRoll_ComplicatedRollWithBadData_Succeeds()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            string result = diceRoller.CalculateRoll(@"d20 + fred -george !r#andom %%text IS $here + 5-3-/d10 + 7-2");
            Assert.AreEqual("(+10) +5 -3 (-3) +7 -2 = 14", result);
        }

        [Test()]
        public void CalculateRoll_RollWithUserSpecified_Succeeds()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            string result = diceRoller.CalculateRoll(@"d20 +4 +/d6 @fred");
            Assert.AreEqual("(+10) +4 (+3) = 17", result);
        }
    }
}
