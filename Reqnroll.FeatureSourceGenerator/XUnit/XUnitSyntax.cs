using Reqnroll.FeatureSourceGenerator.SourceModel;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.XUnit;
internal static class XUnitSyntax
{
    public static readonly NamespaceString XUnitNamespace = new("Xunit");

    internal static QualifiedTypeIdentifier LifetimeInterfaceType(QualifiedTypeIdentifier testFixtureType)
    {
        return XUnitNamespace + new GenericTypeIdentifier(
            new IdentifierString("IClassFixture"),
            ImmutableArray.Create<TypeIdentifier>(
                new NestedTypeIdentifier(
                    testFixtureType.LocalType,
                    new SimpleTypeIdentifier(new IdentifierString("Lifecycle")))));
    }

    internal static AttributeDescriptor SkippableFactAttribute(string displayName)
    {
        return new AttributeDescriptor(
            XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("SkippableFact")),
            namedArguments: ImmutableDictionary.CreateRange(
            [
                new KeyValuePair<IdentifierString, object?>(
                    new IdentifierString("DisplayName"),
                    displayName)
            ]));
    }

    internal static AttributeDescriptor TraitAttribute(string name, string value)
    {
        return new AttributeDescriptor(
            XUnitNamespace + new SimpleTypeIdentifier(new IdentifierString("Trait")),
            ImmutableArray.Create<object?>(name, value));
    }
}
