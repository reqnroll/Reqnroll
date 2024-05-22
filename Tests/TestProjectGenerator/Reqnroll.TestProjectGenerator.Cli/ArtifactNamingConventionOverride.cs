using System;
using Reqnroll.TestProjectGenerator.Conventions;

namespace Reqnroll.TestProjectGenerator.Cli
{
    public class ArtifactNamingConventionOverride : ArtifactNamingConvention
    {
        private readonly SolutionConfiguration _solutionConfiguration;

        public ArtifactNamingConventionOverride(SolutionConfiguration solutionConfiguration)
        {
            _solutionConfiguration = solutionConfiguration;
        }
        public override string GetSolutionName(Guid guid) => _solutionConfiguration.SolutionName ?? base.GetSolutionName(guid);
    }
}
