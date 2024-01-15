using System;
using System.Linq;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class CurrentVersionDriver
    {
        public CurrentVersionDriver()
        {
            var specFlowAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "TechTalk.SpecFlow").SingleOrDefault();
            if (specFlowAssembly != null)
            {
                var specFlowVersion = specFlowAssembly.GetName().Version;

                SpecFlowVersion = new Version(specFlowVersion.Major, specFlowVersion.Minor, 0, 0);
            }
            else
            {
                SpecFlowVersion = new Version(1, 0);
            }
        }

        public string SpecFlowVersionDash => SpecFlowVersion.ToString().Replace(".", "-");

        public Version SpecFlowVersion { get; set; }

        public string SpecFlowNuGetVersion { get; set; }

        public string NuGetVersion { get; set; }

        public string MajorMinorPatchVersion { get; set; }
    }
}
