using System.Collections.Generic;
using System.IO;
using Gherkin.CucumberMessages;
using Gherkin.CucumberMessages.Types;

namespace Reqnroll.Parser
{
    public class CucumberMessagesConverter : ICucumberMessagesConverters
    {
        private IIdGenerator _idGenerator;

        public CucumberMessagesConverter(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }
        public GherkinDocument ConvertToCucumberMessagesGherkinDocument(ReqnrollDocument gherkinDocument)
        {
            var converter = new AstMessagesConverter(_idGenerator);
            var location = Path.Combine(gherkinDocument.DocumentLocation.FeatureFolderPath, Path.GetFileName(gherkinDocument.SourceFilePath));
            return converter.ConvertGherkinDocumentToEventArgs(gherkinDocument, location);
        }

        public Source ConvertToCucumberMessagesSource(ReqnrollDocument gherkinDocument)
        {
            var sourceText = File.ReadAllText(gherkinDocument.SourceFilePath);
            return new Source
            {
                Uri = Path.Combine(gherkinDocument.DocumentLocation.FeatureFolderPath, Path.GetFileName(gherkinDocument.SourceFilePath)),
                Data = sourceText,
                MediaType = "text/x.cucumber.gherkin+plain"
            };
        }

        public IEnumerable<Pickle> ConvertToCucumberMessagesPickles(ReqnrollDocument gherkinDocument)
        {
            var pickleCompiler = new Gherkin.CucumberMessages.Pickles.PickleCompiler(_idGenerator);
            var gd = ConvertToCucumberMessagesGherkinDocument(gherkinDocument);
            return pickleCompiler.Compile(gd);
        }
    }
}
