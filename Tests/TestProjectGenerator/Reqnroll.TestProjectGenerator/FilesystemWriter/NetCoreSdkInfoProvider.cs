using System.Collections.Generic;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public class NetCoreSdkInfoProvider
    {
        /// <summary>
        /// .NET SDK version to be used for .NET Framework and .NET Standard projects.
        /// </summary>
        /// <remarks>
        /// The default SDK should be the latest supported .NET LTS version.
        /// </remarks>
        private static readonly NetCoreSdkInfo DefaultSdk = new("8.0.100");
        private static readonly NetCoreSdkInfo NetCore31 = new("3.1.201");
        private static readonly NetCoreSdkInfo NetCore30 = new("3.0.101");
        private static readonly NetCoreSdkInfo NetCore22 = new("2.2.402");
        private static readonly NetCoreSdkInfo NetCore21 = new("2.1.807");
        private static readonly NetCoreSdkInfo NetCore20 = new("2.1.202");
        private static readonly NetCoreSdkInfo Net80 = new("8.0.100");
        private static readonly NetCoreSdkInfo Net90 = new("9.0.100");

        private readonly IReadOnlyDictionary<TargetFramework, NetCoreSdkInfo> _sdkMappings = new Dictionary<TargetFramework, NetCoreSdkInfo>
        {
            [TargetFramework.Net461] = DefaultSdk,
            [TargetFramework.Net462] = DefaultSdk,
            [TargetFramework.Net471] = DefaultSdk,
            [TargetFramework.Net472] = DefaultSdk,
            [TargetFramework.Net48] = DefaultSdk,
            [TargetFramework.Net481] = DefaultSdk,
            [TargetFramework.Net80] = Net80,
            [TargetFramework.Net90] = Net90,
            [TargetFramework.Netcoreapp31] = NetCore31,
            [TargetFramework.Netcoreapp30] = NetCore30,
            [TargetFramework.Netcoreapp22] = NetCore22,
            [TargetFramework.Netcoreapp21] = NetCore21,
            [TargetFramework.Netcoreapp20] = NetCore20,
            [TargetFramework.NetStandard20] = DefaultSdk
        };

        public NetCoreSdkInfo GetSdkFromTargetFramework(TargetFramework targetFramework)
        {
            if (_sdkMappings.TryGetValue(targetFramework, out var netCoreSdkInfo))
            {
                return netCoreSdkInfo;
            }

            return null;
        }
    }
}
