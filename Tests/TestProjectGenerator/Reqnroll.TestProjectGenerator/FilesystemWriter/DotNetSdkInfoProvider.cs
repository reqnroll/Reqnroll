using System.Collections.Generic;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public class DotNetSdkInfoProvider
    {
        /// <summary>
        /// .NET SDK version to be used for .NET Framework and .NET Standard projects.
        /// </summary>
        /// <remarks>
        /// The default SDK should be the latest supported .NET LTS version.
        /// </remarks>
        private static readonly DotNetSdkInfo DefaultSdk = new("8.0.100");
        private static readonly DotNetSdkInfo Net31 = new("3.1.201");
        private static readonly DotNetSdkInfo Net30 = new("3.0.101");
        private static readonly DotNetSdkInfo Net22 = new("2.2.402");
        private static readonly DotNetSdkInfo Net21 = new("2.1.807");
        private static readonly DotNetSdkInfo Net20 = new("2.1.202");
        private static readonly DotNetSdkInfo Net80 = new("8.0.100");
        private static readonly DotNetSdkInfo Net90 = new("9.0.100");

        private readonly IReadOnlyDictionary<TargetFramework, DotNetSdkInfo> _sdkMappings = new Dictionary<TargetFramework, DotNetSdkInfo>
        {
            [TargetFramework.Net461] = DefaultSdk,
            [TargetFramework.Net462] = DefaultSdk,
            [TargetFramework.Net471] = DefaultSdk,
            [TargetFramework.Net472] = DefaultSdk,
            [TargetFramework.Net48] = DefaultSdk,
            [TargetFramework.Net481] = DefaultSdk,
            [TargetFramework.Net80] = Net80,
            [TargetFramework.Net90] = Net90,
            [TargetFramework.Netcoreapp31] = Net31,
            [TargetFramework.Netcoreapp30] = Net30,
            [TargetFramework.Netcoreapp22] = Net22,
            [TargetFramework.Netcoreapp21] = Net21,
            [TargetFramework.Netcoreapp20] = Net20,
            [TargetFramework.NetStandard20] = DefaultSdk
        };

        public DotNetSdkInfo GetSdkFromTargetFramework(TargetFramework targetFramework)
        {
            if (_sdkMappings.TryGetValue(targetFramework, out var dotNetSdkInfo))
            {
                return dotNetSdkInfo;
            }

            return null;
        }
    }
}
