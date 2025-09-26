#nullable enable
using Gherkin;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.Tracing;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Reqnroll.Generator;

public class TestGenerator(
    ReqnrollConfiguration reqnrollConfiguration,
    ProjectSettings projectSettings,
    IFeatureGeneratorRegistry featureGeneratorRegistry,
    CodeDomHelper codeDomHelper,
    IGherkinParserFactory gherkinParserFactory,
    GeneratorInfo generatorInfo)
    : ITestGenerator
{
    protected readonly ReqnrollConfiguration ReqnrollConfiguration = reqnrollConfiguration ?? throw new ArgumentNullException(nameof(reqnrollConfiguration));
    protected readonly ProjectSettings ProjectSettings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
    protected readonly CodeDomHelper CodeDomHelper = codeDomHelper ?? throw new ArgumentNullException(nameof(codeDomHelper));

    private readonly IFeatureGeneratorRegistry _featureGeneratorRegistry = featureGeneratorRegistry ?? throw new ArgumentNullException(nameof(featureGeneratorRegistry));
    private readonly IGherkinParserFactory _gherkinParserFactory = gherkinParserFactory ?? throw new ArgumentNullException(nameof(gherkinParserFactory));
    private readonly GeneratorInfo _generatorInfo = generatorInfo ?? throw new ArgumentNullException(nameof(generatorInfo));

    [Obsolete("This constructor is obsolete. Use the constructor without ITestHeaderWriter and ITestUpToDateChecker parameters.")]
    public TestGenerator(
        ReqnrollConfiguration reqnrollConfiguration,
        ProjectSettings projectSettings,
        // ReSharper disable once UnusedParameter.Local
        ITestHeaderWriter testHeaderWriter,
        // ReSharper disable once UnusedParameter.Local
        ITestUpToDateChecker testUpToDateChecker,
        IFeatureGeneratorRegistry featureGeneratorRegistry,
        CodeDomHelper codeDomHelper,
        IGherkinParserFactory gherkinParserFactory,
        GeneratorInfo generatorInfo) : this(reqnrollConfiguration, projectSettings, featureGeneratorRegistry, codeDomHelper, gherkinParserFactory, generatorInfo)
    {
    }

    public TestGeneratorResult GenerateTestFile(FeatureFileInput featureFileInput, GenerationSettings settings)
    {
        if (featureFileInput == null) throw new ArgumentNullException(nameof(featureFileInput));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        try
        {
            return GenerateTestFileWithExceptions(featureFileInput, settings);
        }
        catch (ParserException parserException)
        {
            return TestGeneratorResult.FromErrors(
                parserException
                    .GetParserExceptions()
                    .Select(ex => new TestGenerationError(ex.Location?.Line ?? 0, ex.Location?.Column ?? 0, ex.Message)));
        }
        catch (Exception exception)
        {
            return TestGeneratorResult.FromErrors([new TestGenerationError(exception)]);
        }
    }

    protected virtual TestGeneratorResult GenerateTestFileWithExceptions(FeatureFileInput featureFileInput, GenerationSettings settings)
    {
        GenerationTargetLanguage.AssertSupported(ProjectSettings.ProjectPlatformSettings.Language);
        var generationResult = GenerateCodeNamespace(featureFileInput);
        return GenerateCodeFromCodeNamespace(generationResult);
    }

    private TestGeneratorResult GenerateCodeFromCodeNamespace(UnitTestFeatureGenerationResult? generationResult)
    {
        if (generationResult == null)
        {
            // in case of empty feature file, generatedTestCode is empty and featureMessages is null
            return new TestGeneratorResult("", [], null, null);
        }

        var codeProvider = CodeDomHelper.CreateCodeDomProvider();
        using var outputWriter = new IndentProcessingWriter(new StringWriter());
        AddReqnrollHeader(codeProvider, outputWriter);
        AddFileWideUsingStatements(codeProvider, outputWriter);
        codeProvider.GenerateCodeFromNamespace(
            generationResult.CodeNamespace,
            outputWriter,
            new CodeGeneratorOptions
            {
                BracingStyle = "C",
            });
        AddReqnrollFooter(codeProvider, outputWriter);

        outputWriter.Flush();
        var generatedTestCode = outputWriter.ToString();
        if (CodeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB)
            generatedTestCode = FixVb(generatedTestCode);

        return new TestGeneratorResult(generatedTestCode, generationResult.GenerationWarnings, generationResult.FeatureMessages, generationResult.FeatureMessagesResourceName);
    }

    private string FixVb(string generatedTestCode)
    {
        return FixVBNetAsyncMethodDeclarations(FixVBNetGlobalNamespace(generatedTestCode));
    }

    private string FixVBNetGlobalNamespace(string generatedTestCode)
    {
        return generatedTestCode
               .Replace("Global.GlobalVBNetNamespace", "Global")
               .Replace("GlobalVBNetNamespace", "Global");
    }

    static readonly Lazy<Regex> VBFixAsyncFunctionRegex = new(() => new Regex(@"^([^\n]*)Function[ ]*([^(\n]*)(\([^\n]*\)[ ]*As) async([^\n]*)$", RegexOptions.Multiline));
    static readonly Lazy<Regex> VBFixAsyncSubRegex = new(() => new Regex(@"^([^\n]*)Sub[ ]*([^(\n]*)(\([^\n]*\)[ ]*As) async([^\n]*)$", RegexOptions.Multiline));

    /// <summary>
    /// This is a workaround to allow async/await calls in VB.NET. Works in cooperation with CodeDomHelper.MarkCodeMemberMethodAsAsync() method
    /// </summary>
    private string FixVBNetAsyncMethodDeclarations(string generatedTestCode)
    {
        var functionRegex = VBFixAsyncFunctionRegex.Value;
        var subRegex = VBFixAsyncSubRegex.Value;

        var result = functionRegex.Replace(generatedTestCode, "$1 Async Function $2$3$4");
        result = subRegex.Replace(result, "$1 Async Sub $2$3$4");

        return result;
    }
        
    private UnitTestFeatureGenerationResult? GenerateCodeNamespace(FeatureFileInput featureFileInput)
    {
        var reqnrollDocument = ParseReqnrollDocument(featureFileInput);
        if (reqnrollDocument.ReqnrollFeature == null) return null;

        var featureGenerator = _featureGeneratorRegistry.CreateGenerator(reqnrollDocument);
        string targetNamespace = GetTargetNamespace(featureFileInput) ?? "Reqnroll.GeneratedTests";
        var generationResult = featureGenerator.GenerateUnitTestFixture(reqnrollDocument, null, targetNamespace);
        if (generationResult.CodeNamespace == null)
            throw new ReqnrollException("No CodeNamespace has been generated"); // this should never happen

        return generationResult;
    }

    private ReqnrollDocument ParseReqnrollDocument(FeatureFileInput featureFileInput)
    {
        var parser = _gherkinParserFactory.Create(ReqnrollConfiguration.FeatureLanguage);
        using var contentReader = featureFileInput.GetFeatureFileContentReader(ProjectSettings);
        return ParseContent(parser, contentReader, GetReqnrollDocumentLocation(featureFileInput));
    }

    private ReqnrollDocumentLocation GetReqnrollDocumentLocation(FeatureFileInput featureFileInput)
    {
        return new ReqnrollDocumentLocation(
            featureFileInput.GetFullPath(ProjectSettings), 
            GetFeatureFolderPath(featureFileInput.ProjectRelativePath));
    }

    private string? GetFeatureFolderPath(string projectRelativeFilePath)
    {
        var directoryName = Path.GetDirectoryName(projectRelativeFilePath);
        if (string.IsNullOrWhiteSpace(directoryName)) return null;

        return string.Join("/", directoryName.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries));
    }

    protected virtual ReqnrollDocument ParseContent(IGherkinParser parser, TextReader contentReader, ReqnrollDocumentLocation documentLocation)
    {
        return parser.Parse(contentReader, documentLocation);
    }

    protected string? GetTargetNamespace(FeatureFileInput featureFileInput)
    {
        if (!string.IsNullOrEmpty(featureFileInput.CustomNamespace))
            return featureFileInput.CustomNamespace;

        var targetNamespace = string.IsNullOrEmpty(ProjectSettings.DefaultNamespace)
            ? null
            : ProjectSettings.DefaultNamespace;

        var directoryName = Path.GetDirectoryName(featureFileInput.ProjectRelativePath);
        var namespaceExtension = string.IsNullOrEmpty(directoryName) ? null :
            string.Join(".", directoryName.TrimStart('\\', '/', '.').Split('\\', '/').Select(f => f.ToIdentifier()).ToArray());
        if (!string.IsNullOrEmpty(namespaceExtension))
            targetNamespace = targetNamespace == null
                ? namespaceExtension
                : targetNamespace + "." + namespaceExtension;
        return targetNamespace;
    }

    protected void AddFileWideUsingStatements(CodeDomProvider codeProvider,TextWriter outputWriter)
    {
        foreach(var statement in CodeDomHelper.GetFeatureFilewideUsingStatements())
        {
            codeProvider.GenerateCodeFromStatement(statement, outputWriter, null);
        }
    }

    #region Header & Footer
    protected void AddReqnrollHeader(CodeDomProvider codeProvider, TextWriter outputWriter)
    {
        const string reqnrollHeaderTemplate =
            """
            ------------------------------------------------------------------------------
             <auto-generated>
                 This code was generated by Reqnroll (https://reqnroll.net/).
                 Reqnroll Version:{0}
                 Reqnroll Generator Version:{1}

                 Changes to this file may cause incorrect behavior and will be lost if
                 the code is regenerated.
             </auto-generated>
            ------------------------------------------------------------------------------
            """;

        var headerReader = new StringReader(
            string.Format(reqnrollHeaderTemplate, GetCurrentReqnrollVersion(), _generatorInfo.GeneratorVersion));

        while (headerReader.ReadLine() is { } line)
        {
            codeProvider.GenerateCodeFromStatement(new CodeCommentStatement(line), outputWriter, null);
        }

        codeProvider.GenerateCodeFromStatement(CodeDomHelper.GetStartRegionStatement("Designer generated code"), outputWriter, null);
        codeProvider.GenerateCodeFromStatement(CodeDomHelper.GetDisableWarningsPragma(), outputWriter, null);
    }

    protected void AddReqnrollFooter(CodeDomProvider codeProvider, TextWriter outputWriter)
    {
        codeProvider.GenerateCodeFromStatement(CodeDomHelper.GetEnableWarningsPragma(), outputWriter, null);
        codeProvider.GenerateCodeFromStatement(CodeDomHelper.GetEndRegionStatement(), outputWriter, null);
    }

    protected Version GetCurrentReqnrollVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version;
    }
    #endregion
}