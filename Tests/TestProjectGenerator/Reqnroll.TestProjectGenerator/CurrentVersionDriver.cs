using System;
using System.Linq;

namespace Reqnroll.TestProjectGenerator
{
    public class CurrentVersionDriver
    {
        public CurrentVersionDriver()
        {
            var reqnrollAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "Reqnroll").SingleOrDefault();
            if (reqnrollAssembly != null)
            {
                var reqnrollVersion = reqnrollAssembly.GetName().Version;

                ReqnrollVersion = new Version(reqnrollVersion.Major, reqnrollVersion.Minor, 0, 0);
            }
            else
            {
                ReqnrollVersion = new Version(1, 0);
            }
        }

        public string ReqnrollVersionDash => ReqnrollVersion.ToString().Replace(".", "-");

        public Version ReqnrollVersion { get; set; }

        public string ReqnrollNuGetVersion { get; set; }

        public string MajorMinorPatchVersion { get; set; }
    }
}
