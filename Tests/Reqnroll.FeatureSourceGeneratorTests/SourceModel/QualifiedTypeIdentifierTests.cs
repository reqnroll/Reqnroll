using Reqnroll.FeatureSourceGenerator;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class QualifiedTypeIdentifierTests
{
    [Theory]
    [InlineData("Reqnroll", "Parser")]
    [InlineData("Reqnroll", "__Parser")]
    [InlineData("Reqnroll", "X509")]
    public void Constructor_CreatesQualifiedTypeIdentifierFromValidNameAndNamespace(string ns, string name)
    {
        var nsx = new NamespaceString(ns);
        var localType = new SimpleTypeIdentifier(new IdentifierString(name));

        var identifier = new QualifiedTypeIdentifier(nsx, localType);

        identifier.LocalType.Should().Be(localType);
        identifier.Namespace.Should().Be(nsx);
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll.Parser")]
    [InlineData("Reqnroll", "__Parser", "Reqnroll.__Parser")]
    [InlineData("Reqnroll", "X509", "Reqnroll.X509")]
    public void ToString_ReturnsNamespaceAndLocalTypeSeparatedByADot(string ns, string name, string expected)
    {
        var nsx = new NamespaceString(ns);
        var localType = new SimpleTypeIdentifier(new IdentifierString(name));
        var identifier = new QualifiedTypeIdentifier(nsx, localType);

        identifier.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_ThrowsArgumentExceptionWhenUsingAnEmptyNamespace(string ns)
    {
        var localType = new SimpleTypeIdentifier(new IdentifierString("Parser"));
        Func<QualifiedTypeIdentifier> ctr = () => new QualifiedTypeIdentifier(new NamespaceString(ns), localType);

        ctr.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "Parser")]
    [InlineData("Reqnroll", "_Parser", "Reqnroll", "_Parser")]
    public void EqualsTypeIdentifier_ReturnsTrueWhenNamespacesAndLocalTypeMatches(
        string ns1,
        string name1,
        string ns2,
        string name2)
    {
        var typeId1 = new QualifiedTypeIdentifier(new NamespaceString(ns1), new SimpleTypeIdentifier(new IdentifierString(name1)));
        var typeId2 = new QualifiedTypeIdentifier(new NamespaceString(ns2), new SimpleTypeIdentifier(new IdentifierString(name2)));

        typeId1.Equals(typeId2).Should().BeTrue();
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "System", "Parser")]
    [InlineData("Reqnroll", "_Parser", "System", "_Parser")]
    public void EqualsTypeIdentifier_ReturnsFalseWhenNamespaceDoesNotMatch(
        string ns1,
        string name1,
        string ns2,
        string name2)
    {
        var typeId1 = new QualifiedTypeIdentifier(new NamespaceString(ns1), new SimpleTypeIdentifier(new IdentifierString(name1)));
        var typeId2 = new QualifiedTypeIdentifier(new NamespaceString(ns2), new SimpleTypeIdentifier(new IdentifierString(name2)));

        typeId1.Equals(typeId2).Should().BeFalse();
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "parser")]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "_Parser")]
    public void EqualsTypeIdentifier_ReturnsFalseWhenLocalTypeDoesNotMatch(
        string ns1,
        string name1,
        string ns2,
        string name2)
    {
        var typeId1 = new QualifiedTypeIdentifier(new NamespaceString(ns1), new SimpleTypeIdentifier(new IdentifierString(name1)));
        var typeId2 = new QualifiedTypeIdentifier(new NamespaceString(ns2), new SimpleTypeIdentifier(new IdentifierString(name2)));

        typeId1.Equals(typeId2).Should().BeFalse();
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "Parser")]
    [InlineData("Reqnroll", "Internal", "Reqnroll", "Internal")]
    public void GetHashCode_ReturnsSameValueForEquivalentValues(
        string ns1,
        string name1,
        string ns2,
        string name2)
    {
        var typeId1 = new QualifiedTypeIdentifier(new NamespaceString(ns1), new SimpleTypeIdentifier(new IdentifierString(name1)));
        var typeId2 = new QualifiedTypeIdentifier(new NamespaceString(ns2), new SimpleTypeIdentifier(new IdentifierString(name2)));

        typeId1.GetHashCode().Should().Be(typeId2.GetHashCode());
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "Parser", true)]
    [InlineData("Reqnroll", "_Parser", "Reqnroll", "_Parser", true)]
    [InlineData("Reqnroll", "Parser", "System", "Parser", false)]
    [InlineData("Reqnroll", "_Parser", "System", "_Parser", false)]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "parser", false)]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "_Parser", false)]
    public void EqualityOperatorWithTypeIdentifier_ReturnsEquivalenceWithCaseSensitivity(
        string ns1,
        string name1,
        string ns2,
        string name2,
        bool expected)
    {
        var typeId1 = new QualifiedTypeIdentifier(new NamespaceString(ns1), new SimpleTypeIdentifier(new IdentifierString(name1)));
        var typeId2 = new QualifiedTypeIdentifier(new NamespaceString(ns2), new SimpleTypeIdentifier(new IdentifierString(name2)));

        (typeId1 == typeId2).Should().Be(expected);
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "Parser", false)]
    [InlineData("Reqnroll", "_Parser", "Reqnroll", "_Parser", false)]
    [InlineData("Reqnroll", "Parser", "System", "Parser", true)]
    [InlineData("Reqnroll", "_Parser", "System", "_Parser", true)]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "parser", true)]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "_Parser", true)]
    public void InequalityOperatorWithTypeIdentifier_ReturnsNonEquivalenceWithCaseSensitivity(
        string ns1,
        string name1,
        string ns2,
        string name2,
        bool expected)
    {
        var typeId1 = new QualifiedTypeIdentifier(new NamespaceString(ns1), new SimpleTypeIdentifier(new IdentifierString(name1)));
        var typeId2 = new QualifiedTypeIdentifier(new NamespaceString(ns2), new SimpleTypeIdentifier(new IdentifierString(name2)));

        (typeId1 != typeId2).Should().Be(expected);
    }
}
