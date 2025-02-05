using Gherkin.CucumberMessages;
using System.Threading;
namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    public class SeedableIncrementingIdGenerator : IIdGenerator
    {
        public SeedableIncrementingIdGenerator(int seed)
        {
            _counter = seed;
        }

        private int _counter = 0;

        public void SetSeed(int seed)
        {
            _counter = seed;
        }

        public bool HasBeenUsed { get { return _counter > 0; } }

        public string GetNewId()
        {
            // Using thread-safe incrementing in case scenarios are running in parallel
            var nextId = Interlocked.Increment(ref _counter);
            return nextId.ToString();
        }
    }
}
