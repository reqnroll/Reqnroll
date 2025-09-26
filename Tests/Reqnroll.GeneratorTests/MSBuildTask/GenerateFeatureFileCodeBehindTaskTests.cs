using FluentAssertions;
using Moq;
using Reqnroll.Tools.MsBuild.Generation;
using System.Collections.Generic;
using Reqnroll.Analytics;
using Reqnroll.BoDi;
using Xunit;
using Xunit.Abstractions;

namespace Reqnroll.GeneratorTests.MSBuildTask;

public class GenerateFeatureFileCodeBehindTaskTests(ITestOutputHelper output)
{
    private Mock<IFeatureFileCodeBehindGenerator> GetFeatureFileCodeBehindGeneratorMock()
    {
        var generatorMock = new Mock<IFeatureFileCodeBehindGenerator>();
        generatorMock
            .Setup(m => m.GenerateFilesForProject())
            .Returns(new List<FeatureFileCodeBehindGeneratorResult>());
        return generatorMock;
    }

    private Mock<IAnalyticsTransmitter> GetAnalyticsTransmitterMock()
    {
        var analyticsTransmitterMock = new Mock<IAnalyticsTransmitter>();
        analyticsTransmitterMock.Setup(at => at.TransmitReqnrollProjectCompilingEventAsync(It.IsAny<ReqnrollProjectCompilingEvent>()))
                                .Callback(() => { });
        return analyticsTransmitterMock;
    }

    class TestDependencyCustomizations(IAnalyticsTransmitter analyticsTransmitter, IFeatureFileCodeBehindGenerator featureFileCodeBehindGenerator) : IGenerateFeatureFileCodeBehindTaskDependencyCustomizations
    {
        public void CustomizeTaskContainerDependencies(IObjectContainer taskContainer)
        {
            taskContainer.RegisterInstanceAs(analyticsTransmitter);
        }

        public void CustomizeGeneratorContainerDependencies(IObjectContainer generatorContainer)
        {
            generatorContainer.RegisterInstanceAs(featureFileCodeBehindGenerator);
        }
    }

    private IGenerateFeatureFileCodeBehindTaskDependencyCustomizations GetDependencyCustomizations()
    {
        return new TestDependencyCustomizations(GetAnalyticsTransmitterMock().Object, GetFeatureFileCodeBehindGeneratorMock().Object);
    }

    [Fact]
    public void Execute_OnlyRequiredPropertiesAreSet_ShouldWork()
    {
        //ARRANGE
        var generateFeatureFileCodeBehindTask = new GenerateFeatureFileCodeBehindTask
        {
            ProjectPath = "ProjectPath.csproj",
            BuildEngine = new MockBuildEngine(output),
            DependencyCustomizations = GetDependencyCustomizations()
        };

        //ACT
        bool result = generateFeatureFileCodeBehindTask.Execute();

        //ASSERT
        result.Should().BeTrue();
    }

    [Fact]
    public void Execute_AllPropertiesAreSet_ShouldWork()
    {
        //ARRANGE
        var generateFeatureFileCodeBehindTask = new GenerateFeatureFileCodeBehindTask
        {
            RootNamespace = "RootNamespace",
            ProjectPath = "ProjectPath.csproj",
            FeatureFiles = [],
            GeneratorPlugins = [],
            BuildEngine = new MockBuildEngine(output),
            DependencyCustomizations = GetDependencyCustomizations()
        };

        //ACT
        bool result = generateFeatureFileCodeBehindTask.Execute();

        //ASSERT
        result.Should().BeTrue();
    }

    [Fact]
    public void Execute_RootNamespaceEmpty_ShouldWork()
    {
        //ARRANGE
        var generateFeatureFileCodeBehindTask = new GenerateFeatureFileCodeBehindTask
        {
            RootNamespace = "",
            ProjectPath = "ProjectPath.csproj",
            FeatureFiles = [],
            GeneratorPlugins = [],
            BuildEngine = new MockBuildEngine(output),
            DependencyCustomizations = GetDependencyCustomizations()
        };

        //ACT
        bool result = generateFeatureFileCodeBehindTask.Execute();

        //ASSERT
        result.Should().BeTrue();
    }

    [Fact]
    public void Execute_RootNamespaceNull_ShouldWork()
    {
        //ARRANGE
        var generateFeatureFileCodeBehindTask = new GenerateFeatureFileCodeBehindTask
        {
            RootNamespace = null,
            ProjectPath = "ProjectPath.csproj",
            FeatureFiles = [],
            GeneratorPlugins = [],
            BuildEngine = new MockBuildEngine(output),
            DependencyCustomizations = GetDependencyCustomizations()
        };

        //ACT
        bool result = generateFeatureFileCodeBehindTask.Execute();

        //ASSERT
        result.Should().BeTrue();
    }

    [Fact]
    public void Execute_FeatureFilesNotSet_ShouldWork()
    {
        //ARRANGE
        var generateFeatureFileCodeBehindTask = new GenerateFeatureFileCodeBehindTask
        {
            RootNamespace = "RootNamespace",
            ProjectPath = "ProjectPath.csproj",
            GeneratorPlugins = [],
            BuildEngine = new MockBuildEngine(output),
            DependencyCustomizations = GetDependencyCustomizations()
        };

        //ACT
        bool result = generateFeatureFileCodeBehindTask.Execute();

        //ASSERT
        result.Should().BeTrue();
    }

    [Fact]
    public void Execute_GeneratorPluginsNotSet_ShouldWork()
    {
        //ARRANGE
        var generateFeatureFileCodeBehindTask = new GenerateFeatureFileCodeBehindTask
        {
            RootNamespace = "RootNamespace",
            ProjectPath = "ProjectPath.csproj",
            FeatureFiles = [],
            BuildEngine = new MockBuildEngine(output),
            DependencyCustomizations = GetDependencyCustomizations()
        };

        //ACT
        bool result = generateFeatureFileCodeBehindTask.Execute();

        //ASSERT
        result.Should().BeTrue();
    }
}