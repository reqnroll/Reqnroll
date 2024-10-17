using FluentAssertions.Formatting;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator;

internal class AttributeDescriptorFormatter : IValueFormatter
{
    public bool CanHandle(object value) => value is AttributeDescriptor;

    public void Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        var descriptor = (AttributeDescriptor)value;

        if (context.UseLineBreaks)
        {
            formattedGraph.AddFragmentOnNewLine(descriptor.ToString());
        }
        else
        {
            formattedGraph.AddFragment(descriptor.ToString());
        }
    }
}
