using FluentAssertions.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reqnroll.FeatureSourceGenerator;
internal class ParameterSyntaxFormatter : IValueFormatter
{
    public bool CanHandle(object value) => value is ParameterSyntax;

    public void Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        var syntax = (ParameterSyntax)value;

        var typeString = syntax.Type switch
        {
            ArrayTypeSyntax array => $"{array.ElementType}[]",
            not null => syntax.Type.ToString(),
            null => "<null>"
        };

        var s = $"{typeString} {syntax.Identifier}";

        if (context.UseLineBreaks)
        {
            formattedGraph.AddFragmentOnNewLine(s);
        }
        else
        {
            formattedGraph.AddFragment(s);
        }
    }
}
