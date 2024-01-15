using System;
using TechTalk.SpecFlow.TestProjectGenerator.Conventions;

namespace SpecFlow.TestProjectGenerator.Cli
{
    public class SolutionNamingConventionOverride : SolutionNamingConvention
    {
        private readonly SolutionConfiguration _solutionConfiguration;

        public SolutionNamingConventionOverride(SolutionConfiguration solutionConfiguration)
        {
            _solutionConfiguration = solutionConfiguration;
        }
        public override string GetSolutionName(Guid guid) => _solutionConfiguration.SolutionName ?? base.GetSolutionName(guid);
    }
}
