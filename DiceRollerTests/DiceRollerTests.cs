using NUnit.Framework;

using DiceRollerUtils;
using System.Collections.Generic;

namespace DiceRollerTests
{
    [TestFixture()]
    public class DiceRollerTests
    {
        private readonly MockRandomNumberGenerator numberGenerator = new MockRandomNumberGenerator();

        [Test()]
        public void ParseRoll_BasicValidation()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(5);
            numberGenerator.QueuedResults.Enqueue(2);

            var roll = diceRoller.ParseRoll("d10 +2d6 +3");
            Assert.That(roll == "Total: *25*  :  1d10: (*+15*), 2d6: (*+5, +2*), Mod: (*+3*)");
        }

        [Test()]
        public void ParseRoll_BasicCriticalValidation()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(20);
            numberGenerator.QueuedResults.Enqueue(1);

            var roll = diceRoller.ParseRoll("d20 +3 /advantage");
            Assert.That(roll == "_Critical Success!_ Total: *23*  :  1d20: (*+20*), Mod: (*+3*), Thrown out: (*1*)");
        }

        [Test()]
        public void ParseForLabel_VarietyOfLabels()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            Assert.AreEqual("Total:", diceRoller.ParseForLabel(""));
            Assert.AreEqual("Attack:", diceRoller.ParseForLabel("Attack:"));
            Assert.AreEqual("A long label with lots of words:", diceRoller.ParseForLabel("A long label with lots of words:"));
            Assert.AreEqual("Damage:", diceRoller.ParseForLabel("Damage:"));
        }

        [Test()]
        public void BuildDiceBucket_ComplicatedRoll()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            var bucket = diceRoller.BuildDiceBucket(@"/d20 + 5 -3-/d10 + 7-2", RollType.normalRoll, new List<int>());
            Assert.That(bucket.Count == 3);
            Assert.That(bucket[20].Count == 1);
            Assert.That(bucket[20].ToArray()[0] == "+10");

            Assert.That(bucket[10].Count == 1);
            Assert.That(bucket[10].ToArray()[0] == "-3");

            Assert.That(bucket[0].Count == 4);
            Assert.That(bucket[0].ToArray()[0] == "+5");
            Assert.That(bucket[0].ToArray()[1] == "-3");
            Assert.That(bucket[0].ToArray()[2] == "+7");
            Assert.That(bucket[0].ToArray()[3] == "-2");
        }

        [Test()]
        public void BuildDiceBucket_MultipleLists()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            var bucket = diceRoller.BuildDiceBucket(@"d20 +5, d10+3", RollType.normalRoll, new List<int>());
            Assert.That(bucket.Count == 3);
            Assert.That(bucket[20].Count == 1);
            Assert.That(bucket[20].ToArray()[0] == "+10");
            Assert.That(bucket[10].Count == 1);
            Assert.That(bucket[10].ToArray()[0] == "+3");
            Assert.That(bucket[0].Count == 2);
        }

        [Test()]
        public void BuildDiceBucket_DiceMultiplier()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(15);
            numberGenerator.QueuedResults.Enqueue(5);
            numberGenerator.QueuedResults.Enqueue(8);
            numberGenerator.QueuedResults.Enqueue(3);

            var bucket = diceRoller.BuildDiceBucket(@"2d20 + 5 - 3d6", RollType.normalRoll, new List<int>());
            Assert.That(bucket.Count == 3);
            Assert.That(bucket[20].Count == 2);
            Assert.That(bucket[6].Count == 3);
        }

        [Test()]
        public void BuildDiceBucket_LargeDiceMultiplier()
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

            var bucket = diceRoller.BuildDiceBucket("12d20 + 5", RollType.normalRoll, new List<int>());
            Assert.That(bucket.Count == 2);
            Assert.That(bucket[20].Count == 12);
        }

        [Test()]
        public void BuildDiceBucket_ComplicatedRollWithBadData()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(5);

            var bucket = diceRoller.BuildDiceBucket("d20 + fred -george !r#andom %%text IS $here + 5-3-2d10 + 7-2", RollType.normalRoll, new List<int>());
            Assert.That(bucket.Count == 3);
            Assert.That(bucket[20].Count == 1);
            Assert.That(bucket[10].Count == 2);
            Assert.That(bucket[0].Count == 4);
        }

        [Test()]
        public void BuildDiceBucket_RollWithAdvantageMultipleD20s()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            numberGenerator.QueuedResults.Enqueue(6);
            numberGenerator.QueuedResults.Enqueue(20);

            // Right now the way this is coded, each d20 will get multiple rolls, so effectively each roll
            // will be rolled with advantage.  Not sure if this is what we really want in the end or not.

            var throwAway = new List<int>();
            var bucket = diceRoller.BuildDiceBucket("2d20+4 /adv", RollType.withAdvantage, throwAway);
            Assert.That(bucket.Count == 2);
            Assert.That(bucket[20].Count == 2);
            Assert.That(throwAway.Count == 2);
            Assert.That(bucket[20].ToArray()[0] == "+10");
            Assert.That(bucket[20].ToArray()[1] == "+20");
        }

        [Test()]
        public void BuildDiceBucket_RollWithAdvantageMoreComplicatedRoll()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);
            numberGenerator.QueuedResults.Enqueue(4);
            numberGenerator.QueuedResults.Enqueue(6);
            numberGenerator.QueuedResults.Enqueue(2);

            var throwAway = new List<int>();
            var bucket = diceRoller.BuildDiceBucket("d20 +2d8 -7 +2 -1d4 adv", RollType.withAdvantage, throwAway);
            Assert.That(bucket.Count == 4);

            Assert.That(bucket[20].Count == 1);
            Assert.That(bucket[20].ToArray()[0] == "+10");
            Assert.That(throwAway.Count == 1);

            Assert.That(bucket[8].Count == 2);
            Assert.That(bucket[4].Count == 1);
        }

        [Test()]
        public void BuildDiceBucket_RollWithDisadvantage()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(10);
            numberGenerator.QueuedResults.Enqueue(3);

            var throwAway = new List<int>();
            var bucket = diceRoller.BuildDiceBucket("d20+4 /dis", RollType.withDisadvantage, throwAway);
            Assert.That(bucket.Count == 2);

            Assert.That(bucket[20].Count == 1);
            Assert.That(bucket[20].ToArray()[0] == "+3");
            Assert.That(throwAway.Count == 1);
        }

        [Test()]
        public void BuildDiceBucket_BasicFateDice()
        {
            var diceRoller = new DiceRoller(numberGenerator);
            numberGenerator.QueuedResults.Enqueue(1);
            numberGenerator.QueuedResults.Enqueue(2);
            numberGenerator.QueuedResults.Enqueue(3);

            var bucket = diceRoller.BuildDiceBucket("3df", RollType.normalRoll, new List<int>());
            Assert.That(bucket.Count == 1);
            Assert.That(bucket[3].ToArray()[0] == "-1");
            Assert.That(bucket[3].ToArray()[1] == "+0");
            Assert.That(bucket[3].ToArray()[2] == "+1");
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
            Assert.AreEqual(RollType.showUsage, diceRoller.GetRollType("/?"));
            Assert.AreEqual(RollType.showUsage, diceRoller.GetRollType("-?"));
            Assert.AreEqual(RollType.showUsage, diceRoller.GetRollType("help"));
        }


        [Test()]
        public void ParseRoll_BasicCriticalRoll()
        {
            var diceRoller = new DiceRoller(numberGenerator);

            Assert.That(diceRoller.ParseForCritical("+20").Equals("_Critical Success!_ "));
            Assert.That(diceRoller.ParseForCritical("+1").Equals("_Critical Fail!_ "));
            Assert.That(diceRoller.ParseForCritical("+15").Equals(string.Empty));
        }
    }
}