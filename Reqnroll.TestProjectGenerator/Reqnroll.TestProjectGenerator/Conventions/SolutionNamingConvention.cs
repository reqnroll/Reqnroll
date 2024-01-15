using System;

namespace Reqnroll.TestProjectGenerator.Conventions
{
    public class SolutionNamingConvention
    {
        public virtual string GetSolutionName(Guid guid) => $"S{guid.ToString("N").Substring(24)}";
    }
}
