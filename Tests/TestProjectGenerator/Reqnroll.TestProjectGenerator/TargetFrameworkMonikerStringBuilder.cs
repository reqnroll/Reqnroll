using System;
using System.Collections.Generic;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator
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
            [TargetFramework.Net471] = "net471",
            [TargetFramework.Net472] = "net472",
            [TargetFramework.Net48] = "net48",
            [TargetFramework.Net481] = "net481",
            [TargetFramework.NetStandard20] = "netstandard2.0",
            [TargetFramework.Netcoreapp20] = "netcoreapp2.0",
            [TargetFramework.Netcoreapp21] = "netcoreapp2.1",
            [TargetFramework.Netcoreapp22] = "netcoreapp2.2",
            [TargetFramework.Netcoreapp30] = "netcoreapp3.0",
            [TargetFramework.Netcoreapp31] = "netcoreapp3.1",
            [TargetFramework.Net50] = "net5.0",
            [TargetFramework.Net60] = "net6.0",
            [TargetFramework.Net70] = "net7.0",
            [TargetFramework.Net80] = "net8.0",
            [TargetFramework.Net90] = "net9.0",
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
