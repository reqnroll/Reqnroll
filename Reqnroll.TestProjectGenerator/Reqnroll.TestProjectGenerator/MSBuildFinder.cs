using Microsoft.Build.Locator;
using System;
using System.Linq;

namespace Reqnroll.TestProjectGenerator
{
    public class MSBuildFinder
    {
        private static readonly VisualStudioInstanceQueryOptions _visualStudioInstanceQueryOptions =
            new()
            {
                DiscoveryTypes =
                    DiscoveryType.DeveloperConsole
                    | DiscoveryType.VisualStudioSetup
                    | DiscoveryType.DotNetSdk
            };

        public string FindMSBuild()
        {
            var instance = MSBuildLocator
                .QueryVisualStudioInstances(_visualStudioInstanceQueryOptions)
                .FirstOrDefault();

            if (instance == null)
            {
                throw new Exception("Unable to locate MSBuild; Please install the dotnet SDK.");
            }

            return instance.MSBuildPath;
        }
    }
}
