using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.NUnit;
internal static class NUnitSyntax
{
    public static readonly NamespaceString NUnitNamespace = new("NUnit.Framework");

    internal static AttributeDescriptor CategoryAttribute(string tag)
    {
        return new AttributeDescriptor(
            NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Category")),
            ImmutableArray.Create<object?>(tag));
    }

    internal static AttributeDescriptor DescriptionAttribute(string description)
    {
        return new AttributeDescriptor(
            NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Description")),
            ImmutableArray.Create<object?>(description));
    }

    internal static AttributeDescriptor IgnoreAttribute(string reason)
    {
        return new AttributeDescriptor(
            NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Ignore")),
            ImmutableArray.Create<object?>(reason));
    }

    internal static AttributeDescriptor TestAttribute()
    {
        return new AttributeDescriptor(
            NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Test")));
    }

    internal static AttributeDescriptor TestCaseAttribute(
        ImmutableArray<object?> values,
        string? category = null,
        string? ignoreReason = null)
    {
        var namedArguments = new Dictionary<IdentifierString, object?>();

        if (category != null)
        {
            namedArguments.Add(new IdentifierString("Category"), category);
        }

        if (ignoreReason != null)
        {
            namedArguments.Add(new IdentifierString("IgnoreReason"), ignoreReason);
        }

        return new AttributeDescriptor(
            NUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("TestCase")),
            values,
            namedArguments.ToImmutableDictionary());
    }
}
