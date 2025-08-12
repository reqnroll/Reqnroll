using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.Tracing;

namespace Reqnroll.Generator
{
    public class TestGenerator : ErrorHandlingTestGenerator, ITestGenerator
    {
        protected readonly ReqnrollConfiguration reqnrollConfiguration;
        protected readonly ProjectSettings projectSettings;
        protected readonly ITestHeaderWriter testHeaderWriter;
        protected readonly ITestUpToDateChecker testUpToDateChecker;
        protected readonly CodeDomHelper codeDomHelper;
        private readonly IFeatureGeneratorRegistry featureGeneratorRegistry;
        private readonly IGherkinParserFactory gherkinParserFactory;


        public TestGenerator(ReqnrollConfiguration reqnrollConfiguration,
            ProjectSettings projectSettings,
            ITestHeaderWriter testHeaderWriter,
            ITestUpToDateChecker testUpToDateChecker,
            IFeatureGeneratorRegistry featureGeneratorRegistry,
            CodeDomHelper codeDomHelper,
            IGherkinParserFactory gherkinParserFactory)
        {
            if (reqnrollConfiguration == null) throw new ArgumentNullException(nameof(reqnrollConfiguration));
            if (projectSettings == null) throw new ArgumentNullException(nameof(projectSettings));
            if (testHeaderWriter == null) throw new ArgumentNullException(nameof(testHeaderWriter));
            if (testUpToDateChecker == null) throw new ArgumentNullException(nameof(testUpToDateChecker));
            if (featureGeneratorRegistry == null) throw new ArgumentNullException(nameof(featureGeneratorRegistry));
            if (gherkinParserFactory == null) throw new ArgumentNullException(nameof(gherkinParserFactory));

            this.reqnrollConfiguration = reqnrollConfiguration;
            this.testUpToDateChecker = testUpToDateChecker;
            this.featureGeneratorRegistry = featureGeneratorRegistry;
            this.codeDomHelper = codeDomHelper;
            this.testHeaderWriter = testHeaderWriter;
            this.projectSettings = projectSettings;
            this.gherkinParserFactory = gherkinParserFactory;
        }

        protected override TestGeneratorResult GenerateTestFileWithExceptions(FeatureFileInput featureFileInput, GenerationSettings settings)
        {
            if (featureFileInput == null) throw new ArgumentNullException("featureFileInput");
            if (settings == null) throw new ArgumentNullException("settings");

            var generatedTestFullPath = GetTestFullPath(featureFileInput);
            bool? preliminaryUpToDateCheckResult = null;
            if (settings.CheckUpToDate)
            {
                preliminaryUpToDateCheckResult = testUpToDateChecker.IsUpToDatePreliminary(featureFileInput, generatedTestFullPath, settings.UpToDateCheckingMethod);
                if (preliminaryUpToDateCheckResult == true)
                    return new TestGeneratorResult(null, true, null);
            }

            string generatedTestCode = GetGeneratedTestCode(featureFileInput, out IEnumerable<string> generatedWarnings);
            if(string.IsNullOrEmpty(generatedTestCode))
                return new TestGeneratorResult(null, true, generatedWarnings);

            if (settings.CheckUpToDate && preliminaryUpToDateCheckResult != false)
            {
                var isUpToDate = testUpToDateChecker.IsUpToDate(featureFileInput, generatedTestFullPath, generatedTestCode, settings.UpToDateCheckingMethod);
                if (isUpToDate)
                    return new TestGeneratorResult(null, true, generatedWarnings);
            }

            if (settings.WriteResultToFile)
            {
                File.WriteAllText(generatedTestFullPath, generatedTestCode, Encoding.UTF8);
            }

            return new TestGeneratorResult(generatedTestCode, false, generatedWarnings);
        }

        protected string GetGeneratedTestCode(FeatureFileInput featureFileInput, out IEnumerable<string> generationWarnings)
        {
            generationWarnings = Array.Empty<string>();
            using (var outputWriter = new IndentProcessingWriter(new StringWriter()))
            {
                var codeProvider = codeDomHelper.CreateCodeDomProvider();
                var codeNamespace = GenerateTestFileCode(featureFileInput, out generationWarnings);
                if (codeNamespace == null) return "";

                var options = new CodeGeneratorOptions
                                  {
                                      BracingStyle = "C",
                                  };

                AddReqnrollHeader(codeProvider, outputWriter);
                AddFileWideUsingStatements(codeProvider, outputWriter);
                codeProvider.GenerateCodeFromNamespace(codeNamespace, outputWriter, options);
                AddReqnrollFooter(codeProvider, outputWriter);

                outputWriter.Flush();
                var generatedTestCode = outputWriter.ToString();
                if (codeDomHelper.TargetLanguage == CodeDomProviderLanguage.VB)
                    generatedTestCode = FixVb(generatedTestCode);
                return generatedTestCode;
            }
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

        static readonly Lazy<Regex> VBFixAsyncFunctionRegex = new Lazy<Regex>(() => new Regex(@"^([^\n]*)Function[ ]*([^(\n]*)(\([^\n]*\)[ ]*As) async([^\n]*)$", RegexOptions.Multiline));
        static readonly Lazy<Regex> VBFixAsyncSubRegex = new Lazy<Regex>(() => new Regex(@"^([^\n]*)Sub[ ]*([^(\n]*)(\([^\n]*\)[ ]*As) async([^\n]*)$", RegexOptions.Multiline));

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
        
        private CodeNamespace GenerateTestFileCode(FeatureFileInput featureFileInput, out IEnumerable<string> generationWarnings)
        {
            generationWarnings = Array.Empty<string>();
            string targetNamespace = GetTargetNamespace(featureFileInput) ?? "Reqnroll.GeneratedTests";

            var parser = gherkinParserFactory.Create(reqnrollConfiguration.FeatureLanguage);
            ReqnrollDocument reqnrollDocument;
            using (var contentReader = featureFileInput.GetFeatureFileContentReader(projectSettings))
            {
                reqnrollDocument = ParseContent(parser, contentReader, GetReqnrollDocumentLocation(featureFileInput));
            }

            if (reqnrollDocument.ReqnrollFeature == null) return null;

            var featureGenerator = featureGeneratorRegistry.CreateGenerator(reqnrollDocument);

            var codeNamespace = featureGenerator.GenerateUnitTestFixture(reqnrollDocument, null, targetNamespace, out generationWarnings);
            return codeNamespace;
        }

        private ReqnrollDocumentLocation GetReqnrollDocumentLocation(FeatureFileInput featureFileInput)
        {
            return new ReqnrollDocumentLocation(
                featureFileInput.GetFullPath(projectSettings), 
                GetFeatureFolderPath(featureFileInput.ProjectRelativePath));
        }

        private string GetFeatureFolderPath(string projectRelativeFilePath)
        {
            string directoryName = Path.GetDirectoryName(projectRelativeFilePath);
            if (string.IsNullOrWhiteSpace(directoryName)) return null;

            return string.Join("/", directoryName.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries));
        }

        protected virtual ReqnrollDocument ParseContent(IGherkinParser parser, TextReader contentReader, ReqnrollDocumentLocation documentLocation)
        {
            return parser.Parse(contentReader, documentLocation);
        }

        protected string GetTargetNamespace(FeatureFileInput featureFileInput)
        {
            if (!string.IsNullOrEmpty(featureFileInput.CustomNamespace))
                return featureFileInput.CustomNamespace;

            string targetNamespace = projectSettings == null || string.IsNullOrEmpty(projectSettings.DefaultNamespace)
                ? null
                : projectSettings.DefaultNamespace;

            if (targetNamespace != null)
            {
                targetNamespace = string.Join(".", targetNamespace.Split('.').Select(f => f.ToIdentifier()).ToArray());
            }

            var directoryName = Path.GetDirectoryName(featureFileInput.ProjectRelativePath);
            string namespaceExtension = string.IsNullOrEmpty(directoryName) ? null :
                string.Join(".", directoryName.TrimStart('\\', '/', '.').Split('\\', '/').Select(f => f.ToIdentifier()).ToArray());
            if (!string.IsNullOrEmpty(namespaceExtension))
                targetNamespace = targetNamespace == null
                    ? namespaceExtension
                    : targetNamespace + "." + namespaceExtension;
            return targetNamespace;
        }

        public string GetTestFullPath(FeatureFileInput featureFileInput)
        {
            var path = featureFileInput.GetGeneratedTestFullPath(projectSettings);
            if (path != null)
                return path;

            return featureFileInput.GetFullPath(projectSettings) + GenerationTargetLanguage.GetExtension(projectSettings.ProjectPlatformSettings.Language);
        }

        protected void AddFileWideUsingStatements(CodeDomProvider codeProvider,TextWriter outputWriter)
        {
            foreach(var statement in codeDomHelper.GetFeatureFilewideUsingStatements())
            {
                codeProvider.GenerateCodeFromStatement(statement, outputWriter, null);
            }
        }

        #region Header & Footer
        protected override Version DetectGeneratedTestVersionWithExceptions(FeatureFileInput featureFileInput)
        {
            var generatedTestFullPath = GetTestFullPath(featureFileInput);
            return testHeaderWriter.DetectGeneratedTestVersion(featureFileInput.GetGeneratedTestContent(generatedTestFullPath));
        }

        protected void AddReqnrollHeader(CodeDomProvider codeProvider, TextWriter outputWriter)
        {
            const string reqnrollHeaderTemplate = @"------------------------------------------------------------------------------
 <auto-generated>
     This code was generated by Reqnroll (https://www.reqnroll.net/).
     Reqnroll Version:{0}
     Reqnroll Generator Version:{1}

     Changes to this file may cause incorrect behavior and will be lost if
     the code is regenerated.
 </auto-generated>
------------------------------------------------------------------------------";

            var headerReader = new StringReader(string.Format(reqnrollHeaderTemplate,
                GetCurrentReqnrollVersion(),
                TestGeneratorFactory.GeneratorVersion
                ));

            string line;
            while ((line = headerReader.ReadLine()) != null)
            {
                codeProvider.GenerateCodeFromStatement(new CodeCommentStatement(line), outputWriter, null);
            }

            codeProvider.GenerateCodeFromStatement(codeDomHelper.GetStartRegionStatement("Designer generated code"), outputWriter, null);
            codeProvider.GenerateCodeFromStatement(codeDomHelper.GetDisableWarningsPragma(), outputWriter, null);
        }

        protected void AddReqnrollFooter(CodeDomProvider codeProvider, TextWriter outputWriter)
        {
            codeProvider.GenerateCodeFromStatement(codeDomHelper.GetEnableWarningsPragma(), outputWriter, null);
            codeProvider.GenerateCodeFromStatement(codeDomHelper.GetEndRegionStatement(), outputWriter, null);
        }

        protected Version GetCurrentReqnrollVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
        #endregion
    }
}
