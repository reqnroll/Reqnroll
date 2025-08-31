using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal partial class InternalSyntaxToken
{
    internal class InternalSyntaxTokenWithValue<T> : InternalSyntaxToken
    {
        private readonly T _value;

        public InternalSyntaxTokenWithValue(
            SyntaxKind kind,
            string text,
            T value) : base(kind, text)
        {
            _value = value;
        }

        public InternalSyntaxTokenWithValue(
            SyntaxKind kind,
            string text,
            T value,
            bool containsPlaceholder,
            InternalNode? leading,
            InternalNode? trailing) : base(kind, text, leading, trailing)
        {
            _value = value;

            if (containsPlaceholder)
            {
                SetFlag(NodeFlags.ContainsPlaceholder);
            }
        }

        public InternalSyntaxTokenWithValue(
            SyntaxKind kind,
            string text,
            T value,
            bool containsPlaceholder,
            InternalNode? leading,
            InternalNode? trailing,
            ImmutableArray<InternalDiagnostic> diagnostics,
            ImmutableArray<SyntaxAnnotation> annotations) : base(kind, text, leading, trailing, diagnostics, annotations)
        {
            _value = value;

            if (containsPlaceholder) 
            {
                SetFlag(NodeFlags.ContainsPlaceholder);
            }
        }

        public bool ContainsPlaceholder => HasFlag(NodeFlags.ContainsPlaceholder);

        public override object? GetValue() => _value;

        public override InternalNode WithLeadingTrivia(InternalNode? trivia)
        {
            return new InternalSyntaxTokenWithValue<T>(
                Kind,
                _text,
                _value,
                ContainsPlaceholder,
                trivia,
                _trailing,
                GetAttachedDiagnostics(),
                GetAnnotations());
        }

        public override InternalNode WithTrailingTrivia(InternalNode? trivia)
        {
            return new InternalSyntaxTokenWithValue<T>(
                Kind,
                _text,
                _value,
                ContainsPlaceholder,
                _leading,
                trivia,
                GetAttachedDiagnostics(),
                GetAnnotations());
        }

        public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
        {
            return new InternalSyntaxTokenWithValue<T>(
                Kind,
                _text,
                _value,
                ContainsPlaceholder,
                _leading,
                _trailing,
                GetAttachedDiagnostics(),
                annotations);
        }

        public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
        {
            return new InternalSyntaxTokenWithValue<T>(
                Kind,
                _text,
                _value,
                ContainsPlaceholder,
                _leading,
                _trailing,
                diagnostics,
                GetAnnotations());
        }
    }
}
