using Gherkin.CucumberMessages;
using System;
using System.Threading;
namespace Reqnroll.CucumberMessages
{
    public class IdGeneratorFactory
    {
        public static IIdGenerator Create(string previousId)
        {
            if (Guid.TryParse(previousId, out var _))
            {
                return new GuidIdGenerator();
            }
            else
            {
                return new SeedableIncrementingIdGenerator(int.Parse(previousId));
            }
        }
    }

    public class SeedableIncrementingIdGenerator : IIdGenerator
    {
        public SeedableIncrementingIdGenerator(int seed)
        {
            _counter = seed;
        }

        private int _counter = 0;

        public string GetNewId()
        {
            // Using thread-safe incrementing in case scenarios are running in parallel
            var nextId = Interlocked.Increment(ref _counter);
            return nextId.ToString();
        }


        public void Reset()
        {
            _counter = 0;
        }
    }
}
