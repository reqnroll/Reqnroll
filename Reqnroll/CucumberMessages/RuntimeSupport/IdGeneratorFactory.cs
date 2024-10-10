using Gherkin.CucumberMessages;
using Reqnroll.CucumberMessages.Configuration;
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
        public static IIdGenerator Create(IDGenerationStyle style)
        {
            return style switch { 
                IDGenerationStyle.Incrementing => new SeedableIncrementingIdGenerator(0),
                IDGenerationStyle.UUID => new GuidIdGenerator(), 
                _ => throw new NotImplementedException() };
        }
    }
}
