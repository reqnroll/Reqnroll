using System;
using System.IO;
using FluentAssertions;
using Moq;
using Reqnroll.Configuration;
using Xunit;
using Reqnroll.Generator;
using Reqnroll.Generator.Project;
using Reqnroll.Generator.Helpers;

namespace Reqnroll.GeneratorTests;

public class MsBuildProjectReaderTests
{
    private void Should_parse_csproj_file_correctly(string csprojPath, string language, string rootNamespace, string projectName)
    {
        string directoryName = Path.GetDirectoryName(new Uri(GetType().Assembly.Location).LocalPath);
        string projectFilePath = Path.Combine(directoryName!, csprojPath);

        var reqnrollJsonLocatorMock = new Mock<IReqnrollJsonLocator>();
            
        var configurationLoader = new ConfigurationLoader(reqnrollJsonLocatorMock.Object);
        var projectLanguageReader = new ProjectLanguageReader();
        var reader = new ProjectReader(configurationLoader, projectLanguageReader);

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
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_New\sampleCsProjectFile.csproj"), GenerationTargetLanguage.CSharp, "sampleCsProjectFile", "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_CSProj_New_csproj_file_correctly_when_RootNamespace_empty()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_New\sampleCsProjectFile.csproj"), GenerationTargetLanguage.CSharp, null, "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_CSProj_NewComplex_csproj_file_correctly()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_NewComplex\sampleCsProjectFile.csproj"), GenerationTargetLanguage.CSharp, "SampleApp.Web.Tests.UI", "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_CSProj_NewWithExclude_csproj_file_correctly()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_NewWithExclude\sampleCsProjectFile.csproj"), GenerationTargetLanguage.CSharp, "SampleApp.Web.Tests.UI", "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_ToolsVersion12_csproj_file_correctly()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_ToolsVersion_12\sampleCsProjectFile.csproj"), GenerationTargetLanguage.CSharp, "SampleApp.Web.Tests.UI", "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_ToolsVersion12_vbproj_file_correctly()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\VBProj_ToolsVersion_12\sampleCsProjectFile.vbproj"), GenerationTargetLanguage.VB, "SampleApp.Web.Tests.UI", "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_ToolsVersion14_csproj_file_correctly()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_ToolsVersion_14\sampleCsProjectFile.csproj"), GenerationTargetLanguage.CSharp, "SampleApp.Web.Tests.UI", "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_ToolsVersion14_vbproj_file_correctly()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\VBProj_ToolsVersion_14\sampleCsProjectFile.vbproj"), GenerationTargetLanguage.VB, "SampleApp.Web.Tests.UI", "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_ToolsVersion4_csproj_file_correctly()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\CSProj_ToolsVersion_4\sampleCsProjectFile.csproj"), GenerationTargetLanguage.CSharp, "SampleApp.Web.Tests.UI", "sampleCsProjectFile");
    }

    [Fact]
    public void Should_parse_ToolsVersion4_vbproj_file_correctly()
    {
        Should_parse_csproj_file_correctly(PathHelper.SanitizeDirectorySeparatorChar(@"Data\VBProj_ToolsVersion_4\sampleCsProjectFile.vbproj"), GenerationTargetLanguage.VB, "SampleApp.Web.Tests.UI", "sampleCsProjectFile");
    }
}