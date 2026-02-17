using System.IO;
using Moq;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests;

public abstract class TestGeneratorTestsBase
{
    protected ProjectPlatformSettings CSharpPlatformSettings;
    protected ProjectPlatformSettings VBPlatformSettings;
    protected ProjectSettings CSharpProjectSettings;
    protected ProjectSettings VBProjectSettings;
    protected GenerationSettings DefaultSettings;

    public TestGeneratorTestsBase()
    {
        CSharpPlatformSettings = new ProjectPlatformSettings
        {
            Language = GenerationTargetLanguage.CSharp
        };
        VBPlatformSettings = new ProjectPlatformSettings
        {
            Language = GenerationTargetLanguage.VB
        };

        CSharpProjectSettings = new ProjectSettings { ProjectFolder = Path.GetTempPath(), ProjectPlatformSettings = CSharpPlatformSettings };
        VBProjectSettings = new ProjectSettings { ProjectFolder = Path.GetTempPath(), ProjectPlatformSettings = VBPlatformSettings };
        DefaultSettings = new GenerationSettings();
    }

    protected FeatureFileInput CreateSimpleValidFeatureFileInput(string projectRelativeFolderPath = null)
    {
        return CreateSimpleFeatureFileInput(
            """
            Feature: Addition

            @mytag
            Scenario: Add two numbers
                Given I have entered 50 into the calculator
                And I have entered 70 into the calculator
                When I press add
                Then the result should be 120 on the screen

            """,
            projectRelativeFolderPath);
    }

    protected FeatureFileInput CreateSimpleFeatureFileInput(string featureFileContent, string projectRelativeFolderPath = null)
    {
        const string featureFileName = "Dummy.feature";
        string projectRelativeFilePath = projectRelativeFolderPath == null
            ? featureFileName
            : Path.Combine(projectRelativeFolderPath, featureFileName);
        return new FeatureFileInput(projectRelativeFilePath) {FeatureFileContent = featureFileContent};
    }

    protected FeatureFileInput CreateSimpleInvalidFeatureFileInput()
    {
        return CreateSimpleFeatureFileInput(
            """
            Feature: Addition
            Scenario: Add two numbers
                Given I have entered 50 into the calculator
                AndXXX the keyword is misspelled
            """);
    }

    protected TestGenerator CreateTestGenerator()
    {
        return CreateTestGenerator(CSharpProjectSettings);
    }

    protected TestGenerator CreateTestGenerator(ProjectSettings projectSettings)
    {
        ReqnrollConfiguration generatorReqnrollConfiguration = ConfigurationLoader.GetDefault();
        CodeDomHelper codeDomHelper = new CodeDomHelper(CodeDomProviderLanguage.CSharp);
        UnitTestFeatureGenerator unitTestFeatureGenerator = new UnitTestFeatureGenerator(new NUnit3TestGeneratorProvider(codeDomHelper), codeDomHelper, generatorReqnrollConfiguration, new DecoratorRegistryStub());

        var gherkinParserFactory = new ReqnrollGherkinParserFactory();

        var generatorRegistryStub = new Mock<IFeatureGeneratorRegistry>();
        generatorRegistryStub.Setup(r => r.CreateGenerator(It.IsAny<ReqnrollDocument>())).Returns(unitTestFeatureGenerator);

        var generatorInfo = new GeneratorInfo()
        {
            GeneratorVersion = GeneratorInfoProvider.GeneratorVersion,
        };

        return new TestGenerator(generatorReqnrollConfiguration, projectSettings, generatorRegistryStub.Object, codeDomHelper, gherkinParserFactory, generatorInfo);
    }
}