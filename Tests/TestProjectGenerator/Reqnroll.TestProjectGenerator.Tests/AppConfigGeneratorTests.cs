using FluentAssertions;
using Reqnroll.TestProjectGenerator.ConfigurationModel;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator;
using Xunit;

namespace Reqnroll.TestProjectGenerator.Tests
{
    public class AppConfigGeneratorTests
    {
        private readonly AppConfigGenerator _appConfigGenerator;

        public AppConfigGeneratorTests()
        {
            _appConfigGenerator = new AppConfigGenerator(new CurrentVersionDriver());
        }

        [Fact]
        public void FileNameIsAppConfig()
        {
            var configuration = new Configuration();
            var projectFile = _appConfigGenerator.Generate(configuration);
            projectFile.Path.Should().Be("app.config");
        }

        [Fact]
        public void BuildActionIsNone()
        {
            var configuration = new Configuration();
            var projectFile = _appConfigGenerator.Generate(configuration);
            projectFile.BuildAction.Should().Be("None");
        }



        [Fact]
        public void SingleAdditionalStepAssembly()
        {
            var configuration = new Configuration();
            configuration.BindingAssemblies.Add(new BindingAssembly("AdditionalStepAssembly"));

            var projectFile = _appConfigGenerator.Generate(configuration);

            projectFile.Content.Should().Contain("<stepAssemblies>");
            projectFile.Content.Should().Contain("<stepAssembly assembly=\"AdditionalStepAssembly\" />");
            projectFile.Content.Should().Contain("</stepAssemblies>");
        }
    }
}
