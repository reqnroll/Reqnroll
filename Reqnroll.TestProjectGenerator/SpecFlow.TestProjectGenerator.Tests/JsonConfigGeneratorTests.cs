using FluentAssertions;
using TechTalk.SpecFlow.TestProjectGenerator.ConfigurationModel;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.Factories.ConfigurationGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;
using Xunit;

namespace TechTalk.SpecFlow.TestProjectGenerator.Tests
{
    public class JsonConfigGeneratorTests
    {
        private readonly JsonConfigGenerator _jsonConfigGenerator;

        public JsonConfigGeneratorTests()
        {
            _jsonConfigGenerator = new JsonConfigGenerator(new CurrentVersionDriver());
        }

        [Fact]
        public void FileNameIsAppConfig()
        {
            var configuration = new Configuration { UnitTestProvider = TechTalk.SpecFlow.TestProjectGenerator.UnitTestProvider.SpecRun };
            var projectFile = _jsonConfigGenerator.Generate(configuration);
            projectFile.Path.Should().Be("specflow.json");
        }

        [Fact]
        public void BuildActionIsNone()
        {
            var configuration = new Configuration { UnitTestProvider = TechTalk.SpecFlow.TestProjectGenerator.UnitTestProvider.SpecRun };
            var projectFile = _jsonConfigGenerator.Generate(configuration);
            projectFile.BuildAction.Should().Be("None");
        }

        [Fact]
        public void UnitTestProvider()
        {
            var configuration = new Configuration { UnitTestProvider = TechTalk.SpecFlow.TestProjectGenerator.UnitTestProvider.SpecRun };
            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain("\"unitTestProvider\":{\"name\":\"SpecRun\"}");
        }

        [Fact]
        public void SinglePlugin()
        {
            var configuration = new Configuration { UnitTestProvider = TechTalk.SpecFlow.TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new SpecFlowPlugin("SpecRun"));
            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""plugins"":");
            projectFile.Content.Should().Contain(@"{""name"":""SpecRun""}");
        }

        [Fact]
        public void MultiplePlugins()
        {
            var configuration = new Configuration { UnitTestProvider = TechTalk.SpecFlow.TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new SpecFlowPlugin("SpecRun"));
            configuration.Plugins.Add(new SpecFlowPlugin("SpecFlow+Excel"));
            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""plugins"":");
            projectFile.Content.Should().Contain(@"{""name"":""SpecRun""}");
            projectFile.Content.Should().Contain(@"{""name"":""SpecFlow+Excel""}");
        }

        [Fact]
        public void PluginWithPath()
        {
            var configuration = new Configuration { UnitTestProvider = TechTalk.SpecFlow.TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new SpecFlowPlugin("SpecRun", "pathToPluginFolder"));
            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""plugins"":");
            projectFile.Content.Should().Contain(@"{""name"":""SpecRun"",""path"":""pathToPluginFolder""}");
        }

        [Fact]
        public void SingleAdditionalStepAssembly()
        {
            var configuration = new Configuration { UnitTestProvider = TechTalk.SpecFlow.TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.StepAssemblies.Add(new StepAssembly("AdditionalStepAssembly"));

            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""stepAssemblies"":");
            projectFile.Content.Should().Contain(@"[{""assembly"":""AdditionalStepAssembly""}]");
        }
    }
}
