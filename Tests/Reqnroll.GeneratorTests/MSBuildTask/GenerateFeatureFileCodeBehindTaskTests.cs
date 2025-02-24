using FluentAssertions;
using Microsoft.Build.Utilities;
using NSubstitute;
using Reqnroll.Tools.MsBuild.Generation;
using System.Collections.Generic;
using Reqnroll.Analytics;
using Xunit;
using Xunit.Abstractions;

namespace Reqnroll.GeneratorTests.MSBuildTask
{
    public class GenerateFeatureFileCodeBehindTaskTests
    {
        private readonly ITestOutputHelper _output;

        public GenerateFeatureFileCodeBehindTaskTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private IFeatureFileCodeBehindGenerator GetFeatureFileCodeBehindGeneratorMock()
        {
            var generatorMock = Substitute.For<IFeatureFileCodeBehindGenerator>();
            generatorMock
                .GenerateFilesForProject(
                    Arg.Any<List<string>>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(new List<string>());
            return generatorMock;
        }

        private IAnalyticsTransmitter GetAnalyticsTransmitterMock()
        {
            var analyticsTransmitterMock = Substitute.For<IAnalyticsTransmitter>();
            //TODO NSub check
            //analyticsTransmitterMock.TransmitReqnrollProjectCompilingEventAsync(Arg.Any<ReqnrollProjectCompilingEvent>())
            //    .Callback(() => { });
            return analyticsTransmitterMock;
        }

        [Fact]
        public void Execute_OnlyRequiredPropertiesAreSet_ShouldWork()
        {
            //ARRANGE
            var generateFeatureFileCodeBehindTask = new GenerateFeatureFileCodeBehindTask
            {
                ProjectPath = "ProjectPath.csproj",
                BuildEngine = new MockBuildEngine(_output),
                CodeBehindGenerator = GetFeatureFileCodeBehindGeneratorMock(),
                AnalyticsTransmitter = GetAnalyticsTransmitterMock()
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
                FeatureFiles = new TaskItem[0],
                GeneratorPlugins = new TaskItem[0],
                BuildEngine = new MockBuildEngine(_output),
                CodeBehindGenerator = GetFeatureFileCodeBehindGeneratorMock(),
                AnalyticsTransmitter = GetAnalyticsTransmitterMock()
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
                FeatureFiles = new TaskItem[0],
                GeneratorPlugins = new TaskItem[0],
                BuildEngine = new MockBuildEngine(_output),
                CodeBehindGenerator = GetFeatureFileCodeBehindGeneratorMock(),
                AnalyticsTransmitter = GetAnalyticsTransmitterMock()
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
                FeatureFiles = new TaskItem[0],
                GeneratorPlugins = new TaskItem[0],
                BuildEngine = new MockBuildEngine(_output),
                CodeBehindGenerator = GetFeatureFileCodeBehindGeneratorMock(),
                AnalyticsTransmitter = GetAnalyticsTransmitterMock()
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
                GeneratorPlugins = new TaskItem[0],
                BuildEngine = new MockBuildEngine(_output),
                CodeBehindGenerator = GetFeatureFileCodeBehindGeneratorMock(),
                AnalyticsTransmitter = GetAnalyticsTransmitterMock()
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
                FeatureFiles = new TaskItem[0],
                BuildEngine = new MockBuildEngine(_output),
                CodeBehindGenerator = GetFeatureFileCodeBehindGeneratorMock(),
                AnalyticsTransmitter = GetAnalyticsTransmitterMock()
            };

            //ACT
            bool result = generateFeatureFileCodeBehindTask.Execute();

            //ASSERT
            result.Should().BeTrue();
        }
    }
}