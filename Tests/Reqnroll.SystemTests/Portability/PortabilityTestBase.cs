﻿using Reqnroll.TestProjectGenerator.Driver;
using Xunit;
using Xunit.Abstractions;

namespace Reqnroll.SystemTests.Portability;

/// <summary>
/// Supported .NET versions: https://docs.reqnroll.net/latest/installation/compatibility.html#net-versions
/// </summary>
public abstract class PortabilityTestBase : SystemTestBase
{
    protected PortabilityTestBase(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public void GeneratorAllIn_sample_can_be_handled()
    {
        PrepareGeneratorAllInSamples();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [Theory]
    [InlineData(BuildTool.MSBuild)]
    [InlineData(BuildTool.DotnetMSBuild)]
    public void GeneratorAllIn_sample_can_be_compiled_with_MsBuild(BuildTool buildTool)
    {
        PrepareGeneratorAllInSamples();
        _compilationDriver.SetBuildTool(buildTool);

        _compilationDriver.CompileSolution();
    }

    //TODO: test different outcomes: success, failure, pending, undefined, ignored (scenario & scenario outline)
    //TODO: test async steps (async steps are executed in order)
    //TODO: test before/after test run hooks (.NET Framework version of Reqnroll is subscribed to assembly unload)
}
