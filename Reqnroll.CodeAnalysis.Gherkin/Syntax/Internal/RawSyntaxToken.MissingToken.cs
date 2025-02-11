using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal partial class RawSyntaxToken
{
    internal class MissingToken : RawSyntaxToken
    {
        public MissingToken(SyntaxKind kind, string text) : base(kind, text)
        {
            ClearFlag(NodeFlags.IsNotMissing);
        }

        public MissingToken(SyntaxKind kind, string text, RawNode? leading, RawNode? trailing) : base(kind, text, leading, trailing)
        {
            ClearFlag(NodeFlags.IsNotMissing);
        }

        public MissingToken(
            SyntaxKind kind,
            string text,
            RawNode? leading,
            RawNode? trailing,
            ImmutableArray<InternalDiagnostic> diagnostics,
            ImmutableArray<SyntaxAnnotation> annotations) : base(kind, text, leading, trailing, diagnostics, annotations)
        {
            ClearFlag(NodeFlags.IsNotMissing);
        }

        public override RawNode WithLeadingTrivia(RawNode? trivia)
        {
            throw new NotImplementedException();
        }

        public override RawNode WithTrailingTrivia(RawNode? trivia)
        {
            throw new NotImplementedException();
        }
    }
}
