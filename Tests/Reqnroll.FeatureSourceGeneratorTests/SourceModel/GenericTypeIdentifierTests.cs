namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class GenericTypeIdentifierTests
{
    [Theory]
    [InlineData("Parser")]
    [InlineData("__Parser")]
    [InlineData("X509")]
    public void Constructor_CreatesGenericTypeIdentifierFromValidName(string name)
    {
        var nameIdentifier = new IdentifierString(name);

        var identifier = new GenericTypeIdentifier(nameIdentifier, [ new SimpleTypeIdentifier(new IdentifierString("string")) ]);

        identifier.Name.Should().Be(nameIdentifier);
        identifier.IsNullable.Should().BeFalse();
        identifier.TypeArguments.Should().BeEquivalentTo([ new SimpleTypeIdentifier(new IdentifierString("string")) ]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_ThrowsArgumentExceptionWhenUsingAnEmptyName(string name)
    {
        Func<GenericTypeIdentifier> ctr = () => new GenericTypeIdentifier(
            new IdentifierString(name),
            [new SimpleTypeIdentifier(new IdentifierString("string"))]);

        ctr.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ThrowsArgumentExceptionWhenNoTypeArgumentsAreSpecified()
    {
        Func<GenericTypeIdentifier> ctr = () => new GenericTypeIdentifier(new IdentifierString("Parser"), []);

        ctr.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Parser", "Parser")]
    [InlineData("_Parser", "_Parser")]
    public void EqualsGenericTypeIdentifier_ReturnsTrueWhenNameAndGenericTypeArgumentsMatch(
        string name1,
        string name2)
    {
        var typeId1 = new GenericTypeIdentifier(new IdentifierString(name1), [new SimpleTypeIdentifier(new IdentifierString("string"))]);
        var typeId2 = new GenericTypeIdentifier(new IdentifierString(name2), [new SimpleTypeIdentifier(new IdentifierString("string"))]);

        typeId1.Equals(typeId2).Should().BeTrue();
    }

    [Theory]
    [InlineData("Parser", "parser")]
    [InlineData("Parser", "_Parser")]
    public void EqualsGenericTypeIdentifier_ReturnsFalseWhenLocalNameDoesNotMatch(
        string name1,
        string name2)
    {
        var typeId1 = new GenericTypeIdentifier(
            new IdentifierString(name1),
            [new SimpleTypeIdentifier(new IdentifierString("string"))]);

        var typeId2 = new GenericTypeIdentifier(
            new IdentifierString(name2),
            [new SimpleTypeIdentifier(new IdentifierString("string"))]);

        typeId1.Equals(typeId2).Should().BeFalse();
    }

    [Fact]
    public void EqualsGenericTypeIdentifier_ReturnsFalseTypeArgumentsDoNotMatch()
    {
        var typeId1 = new GenericTypeIdentifier(
            new IdentifierString("List"),
            [new SimpleTypeIdentifier(new IdentifierString("string"))]);

        var typeId2 = new GenericTypeIdentifier(
            new IdentifierString("List"),
            [new SimpleTypeIdentifier(new IdentifierString("int"))]);

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
        var typeId1 = new GenericTypeIdentifier(
            new IdentifierString(name1),
            [new SimpleTypeIdentifier(new IdentifierString("int"))]);

        var typeId2 = new GenericTypeIdentifier(
            new IdentifierString(name2),
            [new SimpleTypeIdentifier(new IdentifierString("int"))]);

        typeId1.GetHashCode().Should().Be(typeId2.GetHashCode());
    }

    [Theory]
    [InlineData("Parser", "Parser", true)]
    [InlineData("_Parser", "_Parser", true)]
    [InlineData("_Parser", "_parser", false)]
    [InlineData("Parser", "parser", false)]
    [InlineData("Parser", "_Parser", false)]
    public void EqualityOperatorWithTypeIdentifier_ReturnsEquivalence(
        string name1,
        string name2,
        bool expected)
    {
        var typeId1 = new GenericTypeIdentifier(
            new IdentifierString(name1),
            [new SimpleTypeIdentifier(new IdentifierString("int"))]);

        var typeId2 = new GenericTypeIdentifier(
            new IdentifierString(name2),
            [new SimpleTypeIdentifier(new IdentifierString("int"))]);

        (typeId1 == typeId2).Should().Be(expected);
    }

    [Theory]
    [InlineData("Parser", "Parser", false)]
    [InlineData("_Parser", "_Parser", false)]
    [InlineData("_Parser", "_parser", true)]
    [InlineData("Parser", "parser", true)]
    [InlineData("Parser", "_Parser", true)]
    public void InequalityOperatorWithTypeIdentifier_ReturnsNonEquivalence(
        string name1,
        string name2,
        bool expected)
    {
        var typeId1 = new GenericTypeIdentifier(
            new IdentifierString(name1),
            [new SimpleTypeIdentifier(new IdentifierString("int"))]);

        var typeId2 = new GenericTypeIdentifier(
            new IdentifierString(name2),
            [new SimpleTypeIdentifier(new IdentifierString("int"))]);

        (typeId1 != typeId2).Should().Be(expected);
    }
}
