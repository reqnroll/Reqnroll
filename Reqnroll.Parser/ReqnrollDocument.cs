using Gherkin.Ast;
using System.Collections.Generic;

namespace Reqnroll.Parser
{
    public class ReqnrollDocument : GherkinDocument
    {
        public ReqnrollDocument(ReqnrollFeature feature, IEnumerable<Comment> comments, ReqnrollDocumentLocation documentLocation) : base(feature, comments)
        {
            DocumentLocation = documentLocation;
        }

        public ReqnrollFeature ReqnrollFeature => (ReqnrollFeature) Feature;

        public ReqnrollDocumentLocation DocumentLocation { get; private set; }

        public string SourceFilePath => DocumentLocation?.SourceFilePath;
    }
}
