using System;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    [Serializable]
    public class MissingScenarioDependenciesException : ReqnrollException
    {
        public MissingScenarioDependenciesException()
            : base("No method marked with [ScenarioDependencies] attribute found.")
        {
            // TODO: Fix help link
            HelpLink = @"https://github.com/solidtoken/SpecFlow.DependencyInjection#usage";
        }
    }
}
