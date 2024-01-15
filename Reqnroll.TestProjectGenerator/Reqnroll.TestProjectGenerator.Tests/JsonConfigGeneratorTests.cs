using FluentAssertions;
using Reqnroll.TestProjectGenerator.ConfigurationModel;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator;
using Reqnroll.TestProjectGenerator.NewApi._1_Memory;
using Xunit;

namespace Reqnroll.TestProjectGenerator.Tests
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
            var configuration = new Configuration { UnitTestProvider = Reqnroll.TestProjectGenerator.UnitTestProvider.SpecRun };
            var projectFile = _jsonConfigGenerator.Generate(configuration);
            projectFile.Path.Should().Be("reqnroll.json");
        }

        [Fact]
        public void BuildActionIsNone()
        {
            var configuration = new Configuration { UnitTestProvider = Reqnroll.TestProjectGenerator.UnitTestProvider.SpecRun };
            var projectFile = _jsonConfigGenerator.Generate(configuration);
            projectFile.BuildAction.Should().Be("None");
        }

        [Fact]
        public void UnitTestProvider()
        {
            var configuration = new Configuration { UnitTestProvider = Reqnroll.TestProjectGenerator.UnitTestProvider.SpecRun };
            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain("\"unitTestProvider\":{\"name\":\"SpecRun\"}");
        }

        [Fact]
        public void SinglePlugin()
        {
            var configuration = new Configuration { UnitTestProvider = Reqnroll.TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new ReqnrollPlugin("SpecRun"));
            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""plugins"":");
            projectFile.Content.Should().Contain(@"{""name"":""SpecRun""}");
        }

        [Fact]
        public void MultiplePlugins()
        {
            var configuration = new Configuration { UnitTestProvider = Reqnroll.TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new ReqnrollPlugin("SpecRun"));
            configuration.Plugins.Add(new ReqnrollPlugin("SpecFlow+Excel"));
            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""plugins"":");
            projectFile.Content.Should().Contain(@"{""name"":""SpecRun""}");
            projectFile.Content.Should().Contain(@"{""name"":""SpecFlow+Excel""}");
        }

        [Fact]
        public void PluginWithPath()
        {
            var configuration = new Configuration { UnitTestProvider = Reqnroll.TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.Plugins.Add(new ReqnrollPlugin("SpecRun", "pathToPluginFolder"));
            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""plugins"":");
            projectFile.Content.Should().Contain(@"{""name"":""SpecRun"",""path"":""pathToPluginFolder""}");
        }

        [Fact]
        public void SingleAdditionalStepAssembly()
        {
            var configuration = new Configuration { UnitTestProvider = Reqnroll.TestProjectGenerator.UnitTestProvider.SpecRun };
            configuration.StepAssemblies.Add(new StepAssembly("AdditionalStepAssembly"));

            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""stepAssemblies"":");
            projectFile.Content.Should().Contain(@"[{""assembly"":""AdditionalStepAssembly""}]");
        }
    }
}
