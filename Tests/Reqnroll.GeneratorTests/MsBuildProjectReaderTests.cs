using System;
using System.IO;
using FluentAssertions;
using Moq;
using Reqnroll.Configuration;
using Xunit;
using Reqnroll.Generator;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Project;
using Reqnroll.Generator.Helpers;

namespace Reqnroll.GeneratorTests
{
    public class MsBuildProjectReaderTests
    {
        private void Should_parse_csproj_file_correctly(string csprojPath, string language, string assemblyName, string rootNamespace, string projectName)
        {
            string directoryName = Path.GetDirectoryName(new Uri(GetType().Assembly.Location).LocalPath);
            string projectFilePath = Path.Combine(directoryName, csprojPath);

            var reqnrollJsonLocatorMock = new Mock<IReqnrollJsonLocator>();
            var microsoftExtensionsConfigurationLoaderMock = new Mock<IMS_ConfigurationLoader>();
            
            var configurationLoader = new ConfigurationLoader(reqnrollJsonLocatorMock.Object, microsoftExtensionsConfigurationLoaderMock.Object);
            var generatorConfigurationProvider = new GeneratorConfigurationProvider(configurationLoader);
            var projectLanguageReader = new ProjectLanguageReader();
            var reader = new ProjectReader(generatorConfigurationProvider, projectLanguageReader);

            var reqnrollProjectFile = reader.ReadReqnrollProject(projectFilePath, rootNamespace);

            reqnrollProjectFile.ProjectSettings.DefaultNamespace.Should().Be(rootNamespace);
            reqnrollProjectFile.ProjectSettings.ProjectName.Should().Be(projectName);

            reqnrollProjectFile.ProjectSettings.ProjectPlatformSettings.Language.Should().Be(language);

            reqnrollProjectFile.Configuration.ReqnrollConfiguration.AllowDebugGeneratedFiles.Should().BeFalse();
            reqnrollProjectFile.Configuration.ReqnrollConfiguration.AllowRowTests.Should().BeTrue();
            reqnrollProjectFile.Configuration.ReqnrollConfiguration.FeatureLanguage.Name.Should().Be("en-US");
        }

        [Fact]
        public void Should_parse_CSProj_New_csproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_New\sampleCsProjectfile.csproj"), GenerationTargetLanguage.CSharp, "sampleCsProjectfile", "sampleCsProjectfile", "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_CSProj_New_csproj_file_correctly_when_RootNamespace_empty()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_New\sampleCsProjectfile.csproj"), GenerationTargetLanguage.CSharp, "sampleCsProjectfile", null, "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_CSProj_NewComplex_csproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_NewComplex\sampleCsProjectfile.csproj"), GenerationTargetLanguage.CSharp, "Hacapp.Web.Tests.UI", "Hacapp.Web.Tests.UI", "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_CSProj_NewWithExclude_csproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_NewWithExclude\sampleCsProjectfile.csproj"), GenerationTargetLanguage.CSharp, "Hacapp.Web.Tests.UI", "Hacapp.Web.Tests.UI", "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_ToolsVersion12_csproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_ToolsVersion_12\sampleCsProjectfile.csproj"), GenerationTargetLanguage.CSharp, "Hacapp.Web.Tests.UI", "Hacapp.Web.Tests.UI", "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_ToolsVersion12_vbproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\VBProj_ToolsVersion_12\sampleCsProjectfile.vbproj"), GenerationTargetLanguage.VB, "Hacapp.Web.Tests.UI", "Hacapp.Web.Tests.UI", "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_ToolsVersion14_csproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_ToolsVersion_14\sampleCsProjectfile.csproj"), GenerationTargetLanguage.CSharp, "Hacapp.Web.Tests.UI", "Hacapp.Web.Tests.UI", "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_ToolsVersion14_vbproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\VBProj_ToolsVersion_14\sampleCsProjectfile.vbproj"), GenerationTargetLanguage.VB, "Hacapp.Web.Tests.UI", "Hacapp.Web.Tests.UI", "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_ToolsVersion4_csproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_ToolsVersion_4\sampleCsProjectfile.csproj"), GenerationTargetLanguage.CSharp, "Hacapp.Web.Tests.UI", "Hacapp.Web.Tests.UI", "sampleCsProjectfile");
        }

        [Fact]
        public void Should_parse_ToolsVersion4_vbproj_file_correctly()
        {
            Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\VBProj_ToolsVersion_4\sampleCsProjectfile.vbproj"), GenerationTargetLanguage.VB, "Hacapp.Web.Tests.UI", "Hacapp.Web.Tests.UI", "sampleCsProjectfile");
        }
    }
}