using FluentAssertions;
using Reqnroll.TestProjectGenerator.ConfigurationModel;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator;
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
            var configuration = new Configuration();
            var projectFile = _jsonConfigGenerator.Generate(configuration);
            projectFile.Path.Should().Be("reqnroll.json");
        }

        [Fact]
        public void BuildActionIsNone()
        {
            var configuration = new Configuration();
            var projectFile = _jsonConfigGenerator.Generate(configuration);
            projectFile.BuildAction.Should().Be("None");
        }

        [Fact]
        public void SingleAdditionalStepAssembly()
        {
            var configuration = new Configuration();
            configuration.BindingAssemblies.Add(new BindingAssembly("AdditionalStepAssembly"));

            var projectFile = _jsonConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain(@"""stepAssemblies"":");
            projectFile.Content.Should().Contain(@"[{""assembly"":""AdditionalStepAssembly""}]");
        }
    }
}
