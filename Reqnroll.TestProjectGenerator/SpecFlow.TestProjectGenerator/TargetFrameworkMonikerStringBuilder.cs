using System;
using System.Collections.Generic;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class TargetFrameworkMonikerStringBuilder
    {
        private readonly IReadOnlyDictionary<TargetFramework, string> _targetFrameworkMonikerMappings = new Dictionary<TargetFramework, string>
        {
            [TargetFramework.Net35] = "net35",
            [TargetFramework.Net45] = "net45",
            [TargetFramework.Net452] = "net452",
            [TargetFramework.Net461] = "net461",
            [TargetFramework.Net462] = "net462",
            [TargetFramework.NetStandard20] = "netstandard2.0",
            [TargetFramework.Netcoreapp20] = "netcoreapp2.0",
            [TargetFramework.Netcoreapp21] = "netcoreapp2.1",
            [TargetFramework.Netcoreapp22] = "netcoreapp2.2",
            [TargetFramework.Netcoreapp30] = "netcoreapp3.0",
            [TargetFramework.Netcoreapp31] = "netcoreapp3.1",
            [TargetFramework.Net50] = "net5.0",
            [TargetFramework.Net60] = "net6.0"
        };

        public string BuildTargetFrameworkMoniker(TargetFramework targetFramework)
        {
            if (!_targetFrameworkMonikerMappings.ContainsKey(targetFramework))
            {
                throw new NotSupportedException($"Target framework {targetFramework} is not supported.");
            }

            return _targetFrameworkMonikerMappings[targetFramework];
        }
    }
}
