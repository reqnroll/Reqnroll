using System;
using System.Collections.Generic;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    public class MissingScenarioDependenciesException : ReqnrollException
    {
        public MissingScenarioDependenciesException()
            : this([])
        {
        }

        public MissingScenarioDependenciesException(IList<string> assemblyNames)
            : base(CreateMessage(assemblyNames))
        {
          
        }

        private static string CreateMessage(IList<string> assemblyNames)
        {
            var message = "No method marked with [ScenarioDependencies] attribute found. It should be a (public or non-public) static method.";
            if (assemblyNames.Count > 0)
            {
                message += $" Scanned assemblies: {string.Join(", ", assemblyNames)}.";
            }
            return message;
        }

        public override string HelpLink { get; set; } = "https://go.reqnroll.net/doc-msdi";
    }
}
