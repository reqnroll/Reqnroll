using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal partial class InternalSyntaxToken
{
    internal class MissingToken : InternalSyntaxToken
    {
        public MissingToken(SyntaxKind kind, string text) : base(kind, text)
        {
            ClearFlag(NodeFlags.IsNotMissing);
        }

        public MissingToken(SyntaxKind kind, string text, InternalNode? leading, InternalNode? trailing) : base(kind, text, leading, trailing)
        {
            ClearFlag(NodeFlags.IsNotMissing);
        }

        public MissingToken(
            SyntaxKind kind,
            string text,
            InternalNode? leading,
            InternalNode? trailing,
            ImmutableArray<InternalDiagnostic> diagnostics,
            ImmutableArray<SyntaxAnnotation> annotations) : base(kind, text, leading, trailing, diagnostics, annotations)
        {
            ClearFlag(NodeFlags.IsNotMissing);
        }

        public override InternalNode WithLeadingTrivia(InternalNode? trivia)
        {
            throw new NotImplementedException();
        }

        public override InternalNode WithTrailingTrivia(InternalNode? trivia)
        {
            throw new NotImplementedException();
        }

        public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
        {
            return new MissingToken(Kind, _text, _leading, _trailing, diagnostics, GetAnnotations());
        }
    }
}
