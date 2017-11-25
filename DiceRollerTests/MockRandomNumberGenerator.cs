using System.Collections;

using DiceRollerUtils;

namespace DiceRollerTests
{
    public class MockRandomNumberGenerator : IRandomNumberGenerator
    {
        public Queue QueuedResults = new Queue();

        public int Generate(int minValue, int maxValue)
        {
            return QueuedResults.Count > 0 ? (int)QueuedResults.Dequeue() : -1;
        }
    }
}
