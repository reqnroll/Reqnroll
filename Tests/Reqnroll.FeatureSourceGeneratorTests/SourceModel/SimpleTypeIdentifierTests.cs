using Reqnroll.FeatureSourceGenerator;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class SimpleTypeIdentifierTests
{
    [Theory]
    [InlineData("Parser")]
    [InlineData("__Parser")]
    [InlineData("X509")]
    public void Constructor_CreatesSimpleTypeIdentifierFromValidName(string name)
    {
        var nameIdentifier = new IdentifierString(name);

        var identifier = new SimpleTypeIdentifier(nameIdentifier);

        identifier.Name.Should().Be(nameIdentifier);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_ThrowsArgumentExceptionWhenUsingAnEmptyName(string name)
    {
        Func<SimpleTypeIdentifier> ctr = () => new SimpleTypeIdentifier(new IdentifierString(name));

        ctr.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Parser", "Parser")]
    [InlineData("_Parser", "_Parser")]
    public void EqualsTypeIdentifier_ReturnsTrueWhenLocalNamesMatche(
        string name1,
        string name2)
    {
        var typeId1 = new SimpleTypeIdentifier(new IdentifierString(name1));
        var typeId2 = new SimpleTypeIdentifier(new IdentifierString(name2));

        typeId1.Equals(typeId2).Should().BeTrue();
    }

    [Theory]
    [InlineData("Parser", "parser")]
    [InlineData("Parser", "_Parser")]
    public void EqualsTypeIdentifier_ReturnsFalseWhenLocalNameDoesNotMatch(
        string name1,
        string name2)
    {
        var typeId1 = new SimpleTypeIdentifier(new IdentifierString(name1));
        var typeId2 = new SimpleTypeIdentifier(new IdentifierString(name2));

        typeId1.Equals(typeId2).Should().BeFalse();
    }

    [Theory]
    [InlineData("Parser", "Parser")]
    [InlineData("Internal", "Internal")]
    [InlineData("_1XYZ", "_1XYZ")]
    [InlineData("__Internal", "__Internal")]
    public void GetHashCode_ReturnsSameValueForEquivalentValues(
        string name1,
        string name2)
    {
        var typeId1 = new SimpleTypeIdentifier(new IdentifierString(name1));
        var typeId2 = new SimpleTypeIdentifier(new IdentifierString(name2));

        typeId1.GetHashCode().Should().Be(typeId2.GetHashCode());
    }

    [Theory]
    [InlineData("Parser", "Parser", true)]
    [InlineData("_Parser", "_Parser", true)]
    [InlineData("_Parser", "_parser", false)]
    [InlineData("Parser", "parser", false)]
    [InlineData("Parser", "_Parser", false)]
    public void EqualityOperatorWithTypeIdentifier_ReturnsEquivalenceWithCaseSensitivity(
        string name1,
        string name2,
        bool expected)
    {
        var typeId1 = new SimpleTypeIdentifier(new IdentifierString(name1));
        var typeId2 = new SimpleTypeIdentifier(new IdentifierString(name2));

        (typeId1 == typeId2).Should().Be(expected);
    }

    [Theory]
    [InlineData("Parser", "Parser", false)]
    [InlineData("_Parser", "_Parser", false)]
    [InlineData("_Parser", "_parser", true)]
    [InlineData("Parser", "parser", true)]
    [InlineData("Parser", "_Parser", true)]
    public void InequalityOperatorWithTypeIdentifier_ReturnsNonEquivalenceWithCaseSensitivity(
        string name1,
        string name2,
        bool expected)
    {
        var typeId1 = new SimpleTypeIdentifier(new IdentifierString(name1));
        var typeId2 = new SimpleTypeIdentifier(new IdentifierString(name2));

        (typeId1 != typeId2).Should().Be(expected);
    }
}
