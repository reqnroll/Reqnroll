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
        private static readonly DotNetSdkInfo DefaultSdk = Net80;
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
