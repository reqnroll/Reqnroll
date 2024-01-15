using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.Conventions
{
    public class SolutionNamingConvention
    {
        public virtual string GetSolutionName(Guid guid) => $"S{guid.ToString("N").Substring(24)}";
    }
}
