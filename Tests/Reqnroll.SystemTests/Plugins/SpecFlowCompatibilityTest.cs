using System.IO;
using System.IO.Compression;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.SystemTests.Plugins;

/// <summary>
/// Tests for issue #970 - SpecFlowCompatibility package missing build files
/// </summary>
[TestClass]
public class SpecFlowCompatibilityTest : PluginsTestBase
{
    protected override void TestInitialize()
    {
        base.TestInitialize();
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.NUnit3;
        _projectsDriver.AddNuGetPackage("Reqnroll.SpecFlowCompatibility", _currentVersionDriver.ReqnrollNuGetVersion);
    }

    [TestMethod]
    public void SpecFlowCompatibility_plugin_should_load_successfully()
    {
        // This test validates that the SpecFlowCompatibility NuGet package contains the required
        // build files (build/net462 and build/netstandard2.0 folders) with the generator plugin DLLs.
        // If these files are missing, the project will fail to build with a FileNotFoundException
        // when trying to load Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll
        
        AddSimpleScenario();
        _projectsDriver.AddPassingStepBinding();

        // Compile the project - this will fail if the generator plugin build files are missing
        _compilationDriver.CompileSolution();

        // Verify compilation succeeded
        _compilationResultDriver.CheckSolutionShouldHaveCompiled();
        
        // Execute tests to ensure the plugin works at runtime
        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [TestMethod]
    [TestCategory("PackageValidation")]
    public void SpecFlowCompatibility_package_should_contain_generator_plugin_build_files()
    {
        // This test validates issue #970 - the SpecFlowCompatibility package must contain
        // the generator plugin DLLs in the build folders for both net462 and netstandard2.0
        
        var packagePath = FindSpecFlowCompatibilityPackage();
        packagePath.Should().NotBeNull("SpecFlowCompatibility package should exist");

        using var archive = ZipFile.OpenRead(packagePath!);
        
        // Verify the package contains the generator plugin DLLs in the build folders
        var entries = archive.Entries.Select(e => e.FullName).ToList();
        
        entries.Should().Contain("build/net462/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll",
            "package should contain generator plugin for net462");
        entries.Should().Contain("build/netstandard2.0/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll",
            "package should contain generator plugin for netstandard2.0");
    }

    private string? FindSpecFlowCompatibilityPackage()
    {
        // Try to find the repository root by looking for the solution file
        var currentDir = Path.GetDirectoryName(typeof(SpecFlowCompatibilityTest).Assembly.Location)!;
        var repoRoot = currentDir;
        
        // Navigate up until we find Reqnroll.slnx or reach the root
        while (!string.IsNullOrEmpty(repoRoot) && !File.Exists(Path.Combine(repoRoot, "Reqnroll.slnx")))
        {
            var parent = Path.GetDirectoryName(repoRoot);
            if (parent == repoRoot) // Reached filesystem root
                break;
            repoRoot = parent!;
        }
        
        var packagesFolder = Path.Combine(repoRoot, "GeneratedNuGetPackages");
        
        if (!Directory.Exists(packagesFolder))
            return null;

        return Directory.GetFiles(packagesFolder, "Reqnroll.SpecFlowCompatibility.*.nupkg", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".snupkg"))
            .OrderByDescending(File.GetLastWriteTime)
            .FirstOrDefault();
    }
}
