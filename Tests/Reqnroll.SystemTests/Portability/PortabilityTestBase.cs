using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Driver;
using System;
using System.Runtime.InteropServices;
using Reqnroll.TestProjectGenerator.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll.TestProjectGenerator.FilesystemWriter;
using FluentAssertions;

namespace Reqnroll.SystemTests.Portability;

/// <summary>
/// Supported .NET versions: https://docs.reqnroll.net/latest/installation/compatibility.html#net-versions
/// </summary>
[TestCategory("Portability")]
public abstract class PortabilityTestBase : SystemTestBase
{
    public static IEnumerable<object[]> GetAllUnitTestProviders()
    {
        return [
            [UnitTestProvider.MSTest],
            [UnitTestProvider.NUnit3],
            [UnitTestProvider.xUnit],
        ];
    }

    private void RunSkippableTest(Action test)
    {
        // Mono is not officially supported by xUnit v2 that we use to test. 
        // See https://xunit.net/docs/v3-alpha#v2-changes
        // Related: https://github.com/reqnroll/Reqnroll/issues/132
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
            _testRunConfiguration.UnitTestProvider == UnitTestProvider.xUnit &&
            (_testRunConfiguration.TargetFramework == TargetFramework.Net462 ||
             _testRunConfiguration.TargetFramework == TargetFramework.Net472))
            Assert.Inconclusive("Disabled because xUnit v2 is not supported on Mono");

        try
        {
            test();
        }
        catch (DotNetSdkNotInstalledException ex)
        {
            if (!new ConfigurationDriver().PipelineMode)
                Assert.Inconclusive(ex.ToString());
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetAllUnitTestProviders), DynamicDataSourceType.Method)]
    public void GeneratorAllIn_sample_can_be_handled(UnitTestProvider unitTestProvider)
    {
        _testRunConfiguration.UnitTestProvider = unitTestProvider;
        RunSkippableTest(() =>
        {
            PrepareGeneratorAllInSamples();

            ExecuteTests();

            ShouldAllScenariosPass();
        });
    }

    [TestMethod]
    [TestCategory("MsBuild")]
    [DynamicData(nameof(GetAllUnitTestProviders), DynamicDataSourceType.Method)]
    public void GeneratorAllIn_sample_can_be_compiled_with_MsBuild(UnitTestProvider unitTestProvider)
    {
        _testRunConfiguration.UnitTestProvider = unitTestProvider;
        RunSkippableTest(() =>
        {
            PrepareGeneratorAllInSamples();
            _compilationDriver.SetBuildTool(BuildTool.MSBuild);
            _compilationDriver.CompileSolution();
        });
    }

    [TestMethod]
    [TestCategory("DotnetMSBuild")]
    [DynamicData(nameof(GetAllUnitTestProviders), DynamicDataSourceType.Method)]
    public void GeneratorAllIn_sample_can_be_compiled_with_DotnetMSBuild(UnitTestProvider unitTestProvider)
    {
        _testRunConfiguration.UnitTestProvider = unitTestProvider;
        RunSkippableTest(() =>
        {
            PrepareGeneratorAllInSamples();
            _compilationDriver.SetBuildTool(BuildTool.DotnetMSBuild);

            _compilationDriver.CompileSolution();
        });
    }

    #region Test before/after test run hooks (.NET Framework version of Reqnroll is subscribed to assembly unload)
    [TestMethod]
    [DynamicData(nameof(GetAllUnitTestProviders), DynamicDataSourceType.Method)]
    public void TestRun_hooks_are_executed(UnitTestProvider unitTestProvider)
    {
        _testRunConfiguration.UnitTestProvider = unitTestProvider;
        RunSkippableTest(() =>
        {
            AddSimpleScenario();
            //AddPassingStepBinding();
            if (unitTestProvider == UnitTestProvider.xUnit)
                _projectsDriver.AddStepBinding("StepDefinition", ".*", """
                                                                   var assembly = System.Reflection.Assembly.Load("Reqnroll.xUnit.ReqnrollPlugin");
                                                                   global::Log.LogCustom("assemblyPath", assembly?.Location ?? "<null>");
                                                                   #pragma warning disable SYSLIB0012
                                                                   global::Log.LogCustom("assemblyCodeBase", assembly?.CodeBase ?? "<null>");
                                                                   #pragma warning restore SYSLIB0012
                                                                   var frameworkType = assembly.GetType("Reqnroll.xUnit.ReqnrollPlugin.XunitTestFrameworkWithAssemblyFixture"); // this is null in 2.0.0 and not null in 2.0.1
                                                                   global::Log.LogCustom("frameworkType", frameworkType?.ToString() ?? "<null>");
                                                                   try
                                                                   {
                                                                       var allTypes = assembly.GetTypes();
                                                                   }
                                                                   catch (System.Reflection.ReflectionTypeLoadException ex)
                                                                   {
                                                                       foreach (var loaderException in ex.LoaderExceptions)
                                                                       {
                                                                           global::Log.LogCustom("loaderException", loaderException?.ToString() ?? "<null>");
                                                                       }
                                                                   }
                                                                   """);
            else
                AddPassingStepBinding();

            AddHookBinding("BeforeTestRun");
            AddHookBinding("AfterTestRun");

            var solutionDriver = _testContainer.GetService<SolutionDriver> ();
            var prj = solutionDriver.Projects.Values.First();
            prj.AddFile(new ProjectFile("xunit.runner.json", "Content", """
                                                                        {
                                                                          "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
                                                                          "diagnosticMessages": true,
                                                                          "internalDiagnosticMessages": true
                                                                        }
                                                                        """, CopyToOutputDirectory.CopyIfNewer));

            ExecuteTests();

            Console.WriteLine(_testRunConfiguration.TargetFramework);
            Console.WriteLine(string.Join(",", _bindingDriver.GetActualLogLines("frameworkType")));
            Console.WriteLine(string.Join(",", _bindingDriver.GetActualLogLines("assemblyPath")));
            Console.WriteLine(string.Join(",", _bindingDriver.GetActualLogLines("assemblyCodeBase")));
            Console.WriteLine(string.Join(",", _bindingDriver.GetActualLogLines("loaderException")));
            _bindingDriver.AssertExecutedHooksEqual(
                "BeforeTestRun",
                "AfterTestRun");
            ShouldAllScenariosPass();
            // dummy change to trigger build 14
            if (unitTestProvider == UnitTestProvider.xUnit && _testRunConfiguration.TargetFramework == TargetFramework.Net462) 
                throw new Exception("artificial error to show log of successful test");
        });
    }
    #endregion
}
