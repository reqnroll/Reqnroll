namespace Reqnroll.FeatureSourceGenerator;

public class IdentifierStringTests
{
    [Fact]
    public void DefaultValue_IsEmpty()
    {
        var identifier = default(IdentifierString);

        identifier.IsEmpty.Should().BeTrue();
        identifier.ToString().Should().Be("");
    }

    [Theory]
    [InlineData("Parser")]
    [InlineData("__Parser")]
    [InlineData("X509")]
    public void Constructor_CreatesIdentifierStringFromValidValue(string name)
    {
        var identifier = new IdentifierString(name);

        identifier.IsEmpty.Should().BeFalse();
        identifier.ToString().Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_CreatesEmptyIdentifierStringFromEmptyValue(string name)
    {
        var identifier = new IdentifierString(name);

        identifier.IsEmpty.Should().BeTrue();
        identifier.ToString().Should().Be("");
    }

    [Theory]
    [InlineData(".FeatureSourceGenerator")]
    [InlineData("FeatureSourceGenerator.")]
    [InlineData("Reqnroll.FeatureSourceGenerator")]
    [InlineData("1FeatureSourceGenerator")]
    public void Constructor_ThrowsArgumentExceptionWhenUsingAnInvalidLocalNameValue(string name)
    {
        Func<IdentifierString> ctr = () => new IdentifierString(name);

        ctr.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Parser", "Parser")]
    [InlineData("_Parser", "_Parser")]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData(null, "")]
    public void EqualsIdentifierString_ReturnsTrueWhenValuesMatch(string id1, string id2)
    {
        new IdentifierString(id1).Equals(new IdentifierString(id2)).Should().BeTrue();
    }

    [Theory]
    [InlineData("Parser", "parser")]
    [InlineData("Parser", "_Parser")]
    public void EqualsIdentifierString_ReturnsFalseWhenValuesDoNotMatch(string id1, string id2)
    {
        new IdentifierString(id1).Equals(new IdentifierString(id2)).Should().BeFalse();
    }

    [Theory]
    [InlineData("Parser", "Parser", true)]
    [InlineData("_Parser", "_Parser", true)]
    [InlineData("_Parser", "_parser", false)]
    [InlineData(null, "", true)]
    [InlineData(null, null, true)]
    [InlineData("", null, true)]
    public void EqualsString_ReturnsCaseSensitiveEquivalence(string? id, string? identifier, bool expected)
    {
        new IdentifierString(id).Equals(identifier).Should().Be(expected);
    }

    [Theory]
    [InlineData("Parser", "Parser")]
    [InlineData("Internal", "Internal")]
    [InlineData("_1XYZ", "_1XYZ")]
    [InlineData("__Internal", "__Internal")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void GetHashCode_ReturnsSameValueForEquivalentValues(string? id1, string? id2)
    {
        new IdentifierString(id1).GetHashCode().Should().Be(new IdentifierString(id2).GetHashCode());
    }

    [Theory]
    [InlineData("Parser", "Parser", true)]
    [InlineData("Parser", "parser", false)]
    [InlineData("_Parser", "_Parser", true)]
    [InlineData("_Parser", "_parser", false)]
    [InlineData(null, "", true)]
    [InlineData(null, null, true)]
    [InlineData("", null, true)]
    public void EqualityOperatorWithString_ReturnsEquivalenceWithCaseSensitivity(string? id, string? identifier, bool expected)
    {
        (new IdentifierString(id) == identifier).Should().Be(expected);
    }

    [Theory]
    [InlineData("Parser",  "Parser", true)]
    [InlineData("_Parser" , "_Parser", true)]
    [InlineData("_Parser","_parser", false)]
    [InlineData("Parser",  "parser", false)]
    [InlineData("Parser",  "_Parser", false)]
    public void EqualityOperatorWithIdentifierString_ReturnsEquivalenceWithCaseSensitivity(string? id1, string? id2, bool expected)
    {
        (new IdentifierString(id1) == new IdentifierString(id2)).Should().Be(expected);
    }

    [Theory]
    [InlineData("Parser", "Parser", false)]
    [InlineData("Parser", "parser", true)]
    [InlineData("_Parser", "_Parser", false)]
    [InlineData("_Parser", "_parser", true)]
    [InlineData(null, "", false)]
    [InlineData(null, null, false)]
    [InlineData("", null, false)]
    public void InequalityOperatorWithString_ReturnsNonEquivalenceWithCaseSensitivity(
        string? id,
        string? identifier,
        bool expected)
    {
        (new IdentifierString(id) != identifier).Should().Be(expected);
    }

    [Theory]
    [InlineData("Parser", "Parser", false)]
    [InlineData("Parser", "parser", true)]
    [InlineData("_Parser", "_Parser", false)]
    [InlineData("_Parser", "_parser", true)]
    [InlineData(null, "", false)]
    [InlineData(null, null, false)]
    [InlineData("", null, false)]
    public void InequalityOperatorWithIdentifierString_ReturnsNonEquivalenceWithCaseSensitivity(
        string? id1,
        string? id2,
        bool expected)
    {
        (new IdentifierString(id1) != new IdentifierString(id2)).Should().Be(expected);
    }
}
