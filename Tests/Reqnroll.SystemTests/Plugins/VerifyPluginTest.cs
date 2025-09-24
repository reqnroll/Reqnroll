using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests.Plugins;

[TestClass]
public class VerifyPluginTest : PluginsTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.xUnit;
        _projectsDriver.AddNuGetPackage("Reqnroll.Verify", _currentVersionDriver.ReqnrollNuGetVersion);
        // use the minimum version of xunit that is compatible with the Verify plugin
        _solutionDriver.DefaultProject.UpdateNuGetPackage("xunit", "2.9.3");
    }

    [TestMethod]
    public void Verify_should_work_with_Reqnroll()
    {
        AddFeatureFileFromResource("VerifyPlugin/Feature.feature", resourceGroup: "Plugins");
        AddFeatureFileFromResource("VerifyPlugin/Parallel Feature 1.feature", resourceGroup: "Plugins");
        AddFeatureFileFromResource("VerifyPlugin/Parallel Feature 2.feature", resourceGroup: "Plugins");
        AddBindingClassFromResource("VerifyPlugin/StepDefinitions.cs", resourceGroup: "Plugins");
        AddContentFileFromResource("VerifyPlugin/Verify Parallel feature #1.Check if Verify uses the correct paths when ran in parallel 1.verified.txt", resourceGroup: "Plugins");
        AddContentFileFromResource("VerifyPlugin/Verify Parallel feature #2.Check if Verify uses the correct paths when ran in parallel 2.verified.txt", resourceGroup: "Plugins");
        AddContentFileFromResource("VerifyPlugin/Verify Test.Check if Verify is working with Example Tables_1.verified.txt", resourceGroup: "Plugins");
        AddContentFileFromResource("VerifyPlugin/Verify Test.Check if Verify is working with Example Tables_2.verified.txt", resourceGroup: "Plugins");
        AddContentFileFromResource("VerifyPlugin/Verify Test.Check if Verify is working with global registered path info.verified.txt", resourceGroup: "Plugins");
        AddContentFileFromResource("VerifyPlugin/Verify Test.Check if Verify is working with multiple scenario parameters_1_a.verified.txt", resourceGroup: "Plugins");
        AddContentFileFromResource("VerifyPlugin/Verify Test.Check if Verify is working with multiple scenario parameters_2_b.verified.txt", resourceGroup: "Plugins");
        AddContentFileFromResource("VerifyPlugin/Verify Test.Check if Verify is working.verified.txt", resourceGroup: "Plugins");

        // avoid parallel execution for 'Feature.feature' to provide have basic tests
        _configurationFileDriver.AddNonParallelizableMarkerForTag("DoNotParallelize");

        // to avoid popup of diff tool
        Environment.SetEnvironmentVariable("DiffEngine_Disabled", "true", EnvironmentVariableTarget.Process);
        // verify has a feature to detect running with NCrunch, but this detects the project folder incorrectly when the Reqnroll tests are run with NCrunch
        Environment.SetEnvironmentVariable("NCrunch.OriginalProjectPath", _testProjectFolders.ProjectFolder + Path.DirectorySeparatorChar, EnvironmentVariableTarget.Process);

        ExecuteTests();

        // the tests should pass according to the content of the .verified.txt files
        ShouldAllScenariosPass();
    }
}
