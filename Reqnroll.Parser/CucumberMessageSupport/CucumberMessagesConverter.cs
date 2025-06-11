using System.Collections.Generic;
using System.IO;
using Gherkin.CucumberMessages;
using Gherkin.CucumberMessages.Types;

namespace Reqnroll.Parser.CucmberMessageSupport
{
     /// <summary>
     /// Utility class that converts Reqnroll AST types in to CucumberMessages.Types types.
     /// It uses two classes from the Gherkin project: AstMessagesConverter and PickleCompiler
     /// 
     /// Once the Gherkin project implementation directly emits CucumberMessages (eliminating the use of the Gherkin.CucumberMessages.Types namespace), this class can be removed
     /// </summary>
    public class CucumberMessagesConverter : ICucumberMessagesConverters
    {
        private IIdGenerator _idGenerator;

        public CucumberMessagesConverter(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        // This method transforms an AST ReqnrollDocument into a CucumberMessages.Types.GherkinDocument
        // Before doing so, it patches any missing Location elements in the AST. These might be missing because our ExternalData Plugin does not emit Location elements for the Example table rows it generates
        // 
        public GherkinDocument ConvertToCucumberMessagesGherkinDocument(ReqnrollDocument gherkinDocument)
        {
            var NullLocationPatcher = new PatchMissingLocationElementsTransformation();
            var gherkinDocumentWithLocation = NullLocationPatcher.TransformDocument(gherkinDocument);
            var converter = new AstMessagesConverter(_idGenerator);
            var location = Path.Combine(gherkinDocument.DocumentLocation.FeatureFolderPath, Path.GetFileName(gherkinDocument.SourceFilePath));
            return converter.ConvertGherkinDocumentToEventArgs(gherkinDocumentWithLocation, location);
        }

        public Source ConvertToCucumberMessagesSource(ReqnrollDocument gherkinDocument)
        {
            if (File.Exists(gherkinDocument.SourceFilePath))
            {
                var sourceText = File.ReadAllText(gherkinDocument.SourceFilePath);
                return new Source
                {
                    Uri = Path.Combine(gherkinDocument.DocumentLocation.FeatureFolderPath, Path.GetFileName(gherkinDocument.SourceFilePath)),
                    Data = sourceText,
                    MediaType = "text/x.cucumber.gherkin+plain"
                };
            }
            else
            {
                return new Source
                {
                    Uri = "Unknown",
                    Data = $"Source Document: {gherkinDocument.SourceFilePath} not found.",
                    MediaType = "text/x.cucumber.gherkin+plain"
                };

            }
        }

        public IEnumerable<Pickle> ConvertToCucumberMessagesPickles(GherkinDocument gherkinDocument)
        {
            var pickleCompiler = new Gherkin.CucumberMessages.Pickles.PickleCompiler(_idGenerator);
            return pickleCompiler.Compile(gherkinDocument);
        }
    }
}
