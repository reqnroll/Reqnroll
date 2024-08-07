using Reqnroll.SystemTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests
{
    public class CucumberCompatibilityTestBase : SystemTestBase
    {

        protected void AddCucumberMessagePlugIn()
        {
            _projectsDriver.AddNuGetPackage("Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin", "2.1.0-local");
        }
    }
}
