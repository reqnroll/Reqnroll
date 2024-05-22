using System;
using System.Collections.Generic;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator
{
    public class TargetFrameworkVersionStringBuilder
    {
        private readonly IReadOnlyDictionary<TargetFramework, string> _targetFrameworkMonikerMappings = new Dictionary<TargetFramework, string>
        {
            [TargetFramework.Net35] = "v3.5",
            [TargetFramework.Net45] = "v4.5",
            [TargetFramework.Net452] = "v4.5.2",
            [TargetFramework.Net461] = "v4.6.1",
            [TargetFramework.Net462] = "v4.6.2",
        };

        public string BuildTargetFrameworkVersion(TargetFramework targetFramework)
        {
            if (_targetFrameworkMonikerMappings.TryGetValue(targetFramework, out string result))
            {
                return result;
            }

            throw new InvalidOperationException("Only .NET Framework target frameworks are supported.");
        }
    }
}
