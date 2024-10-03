using Gherkin.CucumberMessages;
using System;
namespace Reqnroll.CucumberMessages.RuntimeSupport
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
}
