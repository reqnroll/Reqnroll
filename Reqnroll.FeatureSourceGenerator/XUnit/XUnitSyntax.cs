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
}
