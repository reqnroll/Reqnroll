﻿namespace Reqnroll.FeatureSourceGenerator;

public class TypeIdentifierTests
{
    [Fact]
    public void DefaultValue_IsEmpty()
    {
        var identifier = default(TypeIdentifier);

        identifier.IsEmpty.Should().BeTrue();
        identifier.LocalName.IsEmpty.Should().BeTrue();
        identifier.Namespace.IsEmpty.Should().BeTrue();
        identifier.ToString().Should().Be("");
    }

    [Theory]
    [InlineData("Parser")]
    [InlineData("__Parser")]
    [InlineData("X509")]
    public void Constructor_CreatesTypeIdentifierFromValidName(string name)
    {
        var identifier = new TypeIdentifier(new IdentifierString(name));

        identifier.IsEmpty.Should().BeFalse();
        identifier.LocalName.Should().Be(name);
        identifier.Namespace.IsEmpty.Should().BeTrue();
        identifier.ToString().Should().Be(name);
    }

    [Theory]
    [InlineData("Reqnroll", "Parser")]
    [InlineData("Reqnroll", "__Parser")]
    [InlineData("Reqnroll", "X509")]
    public void Constructor_CreatesTypeIdentifierFromValidNameAndNamespace(string ns, string name)
    {
        var nsx = new NamespaceString(ns);

        var identifier = new TypeIdentifier(nsx, new IdentifierString(name));

        identifier.IsEmpty.Should().BeFalse();
        identifier.LocalName.Should().Be(name);
        identifier.Namespace.Should().Be(nsx);
        identifier.ToString().Should().Be($"{ns}.{name}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_CreatesEmptyTypeIdentifierFromEmptyNameValue(string name)
    {
        var identifier = new TypeIdentifier(new IdentifierString(name));

        identifier.IsEmpty.Should().BeTrue();
        identifier.LocalName.IsEmpty.Should().BeTrue();
        identifier.Namespace.IsEmpty.Should().BeTrue();
        identifier.ToString().Should().Be("");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_ThrowsArgumentExceptionWhenUsingAnEmptyNameWithANonEmptyNamespace(string name)
    {
        Func<TypeIdentifier> ctr = () => new TypeIdentifier(new NamespaceString("Reqnroll"), new IdentifierString(name));

        ctr.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(".FeatureSourceGenerator")]
    [InlineData("FeatureSourceGenerator.")]
    [InlineData("Reqnroll.FeatureSourceGenerator")]
    [InlineData("1FeatureSourceGenerator")]
    public void Constructor_ThrowsArgumentExceptionWhenUsingAnInvalidLocalNameValue(string name)
    {
        Func<TypeIdentifier> ctr = () => new TypeIdentifier(new IdentifierString(name));

        ctr.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "Parser")]
    [InlineData("Reqnroll", "_Parser", "Reqnroll", "_Parser")]
    public void EqualsTypeIdentifier_ReturnsTrueWhenNamespacesAndLocalNameMatches(
        string ns1,
        string name1,
        string ns2,
        string name2)
    {
        var typeId1 = new TypeIdentifier(new NamespaceString(ns1), new IdentifierString(name1));
        var typeId2 = new TypeIdentifier(new NamespaceString(ns2), new IdentifierString(name2));

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
        var typeId1 = new TypeIdentifier(new NamespaceString(ns1), new IdentifierString(name1));
        var typeId2 = new TypeIdentifier(new NamespaceString(ns2), new IdentifierString(name2));

        typeId1.Equals(typeId2).Should().BeFalse();
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "parser")]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "_Parser")]
    public void EqualsTypeIdentifier_ReturnsFalseWhenLocalNameDoesNotMatch(
        string ns1,
        string name1,
        string ns2,
        string name2)
    {
        var typeId1 = new TypeIdentifier(new NamespaceString(ns1), new IdentifierString(name1));
        var typeId2 = new TypeIdentifier(new NamespaceString(ns2), new IdentifierString(name2));

        typeId1.Equals(typeId2).Should().BeFalse();
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll.Parser", true)]
    [InlineData("Reqnroll", "_Parser", "Reqnroll._Parser", true)]
    [InlineData(null, "_Parser", "_Parser", true)]
    [InlineData(null, null, "", true)]
    [InlineData(null, null, null, true)]
    [InlineData("", "", null, true)]
    public void EqualsString_ReturnsCaseSensitiveEquivalence(string ns, string name, string identifier, bool expected)
    {
        new TypeIdentifier(new NamespaceString(ns), new IdentifierString(name)).Equals(identifier).Should().Be(expected);
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll", "Parser")]
    [InlineData("Reqnroll", "Internal", "Reqnroll", "Internal")]
    [InlineData(null, "_1XYZ", null, "_1XYZ")]
    [InlineData("", "__Internal", "", "__Internal")]
    [InlineData("", "", "", "")]
    [InlineData(null, null, "", "")]
    public void GetHashCode_ReturnsSameValueForEquivalentValues(
        string ns1,
        string name1,
        string ns2,
        string name2)
    {
        var typeId1 = new TypeIdentifier(new NamespaceString(ns1), new IdentifierString(name1));
        var typeId2 = new TypeIdentifier(new NamespaceString(ns2), new IdentifierString(name2));

        typeId1.GetHashCode().Should().Be(typeId2.GetHashCode());
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll.Parser", true)]
    [InlineData("Reqnroll", "Parser", "Reqnroll.parser", false)]
    [InlineData("Reqnroll", "_Parser", "Reqnroll._Parser", true)]
    [InlineData(null, "_Parser", "_Parser", true)]
    [InlineData(null, "_Parser", "_parser", false)]
    [InlineData(null, null, "", true)]
    [InlineData(null, null, null, true)]
    [InlineData("", "", null, true)]
    public void EqualityOperatorWithString_ReturnsEquivalenceWithCaseSensitivity(string ns, string name, string id, bool expected)
    {
        (new TypeIdentifier(new NamespaceString(ns), new IdentifierString(name)) == id).Should().Be(expected);
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
        var typeId1 = new TypeIdentifier(new NamespaceString(ns1), new IdentifierString(name1));
        var typeId2 = new TypeIdentifier(new NamespaceString(ns2), new IdentifierString(name2));

        (typeId1 == typeId2).Should().Be(expected);
    }

    [Theory]
    [InlineData("Reqnroll", "Parser", "Reqnroll.Parser", false)]
    [InlineData("Reqnroll", "Parser", "Reqnroll.parser", true)]
    [InlineData("Reqnroll", "_Parser", "Reqnroll._Parser", false)]
    [InlineData(null, "_Parser", "_Parser", false)]
    [InlineData(null, "_Parser", "_parser", true)]
    [InlineData(null, null, "", false)]
    [InlineData(null, null, null, false)]
    [InlineData("", "", null, false)]
    public void InequalityOperatorWithString_ReturnsNonEquivalenceWithCaseSensitivity(
        string ns,
        string name,
        string id,
        bool expected)
    {
        (new TypeIdentifier(new NamespaceString(ns), new IdentifierString(name)) != id).Should().Be(expected);
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
        var typeId1 = new TypeIdentifier(new NamespaceString(ns1), new IdentifierString(name1));
        var typeId2 = new TypeIdentifier(new NamespaceString(ns2), new IdentifierString(name2));

        (typeId1 != typeId2).Should().Be(expected);
    }
}
