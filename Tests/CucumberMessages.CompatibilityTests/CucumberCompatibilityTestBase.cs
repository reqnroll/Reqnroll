using FluentAssertions;
using Reqnroll.SystemTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        protected void AddBindingClassFromResource(string fileName, string? prefix = null, Assembly? assemblyToLoadFrom = null)
        {
            var bindingCLassFileContent = _testFileManager.GetTestFileContent(fileName, prefix, assemblyToLoadFrom);
            AddBindingClass(bindingCLassFileContent);
        }

        protected void ShouldAllScenariosPend(int? expectedNrOfTestsSpec = null)
        {
            int expectedNrOfTests = ConfirmAllTestsRan(expectedNrOfTestsSpec);
            _vsTestExecutionDriver.LastTestExecutionResult.Pending.Should().Be(expectedNrOfTests, "all tests should pend");
        }
    }
}
