using System;

namespace Reqnroll.TestProjectGenerator.Conventions
{
    public class ArtifactNamingConvention
    {
        public virtual string GetSolutionName(Guid guid) => $"S{guid.ToString("N").Substring(24)}";

        public string GetRunName(Guid guid) => $"R{guid.ToString("N").Substring(24)}";
    }
}
