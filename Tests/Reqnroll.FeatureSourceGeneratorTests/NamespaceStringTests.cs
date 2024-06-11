namespace Reqnroll.FeatureSourceGenerator;

public class NamespaceStringTests
{
    [Fact]
    public void DefaultInstance_IsEmpty()
    {
        var ns = default(NamespaceString);

        ns.IsEmpty.Should().BeTrue();
        ns.ToString().Should().Be("");
    }

    [Theory]
    [InlineData("Reqnoll")]
    [InlineData("Reqnoll.FeatureSourceGenerator")]
    [InlineData("__Internal")]
    [InlineData("_1XYZ")]
    [InlineData("Reqnroll.__Internal")]
    public void Constructor_CreatesNamespaceValueFromValidString(string s)
    {
        var ns = new NamespaceString(s);

        ns.ToString().Should().Be(s);
        ns.IsEmpty.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Construtor_CreatesEmptyNamespaceValueFromEmptyString(string s)
    {
        var ns = new NamespaceString(s);

        ns.ToString().Should().Be("");
        ns.IsEmpty.Should().BeTrue();
    }

    [Theory]
    [InlineData(".Reqnroll")]
    [InlineData("Reqnroll.")]
    [InlineData(".Reqnroll.FeatureSourceGenerator")]
    [InlineData("Reqnroll.FeatureSourceGenerator.")]
    [InlineData("Reqnroll.FeatureSourceGenerator.1")]
    [InlineData("Reqnroll..FeatureSourceGenerator")]
    public void Constructor_ThrowsArgumentExceptionWhenStringIsNotValidAsNamespace(string s)
    {
        Func<NamespaceString> ctr = () => new NamespaceString(s);
        ctr.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Reqnroll", "Reqnroll", true)]
    [InlineData("Reqnroll", "reqnroll", false)]
    [InlineData("Reqnroll.FeatureSourceGenerator", "Reqnroll.FeatureSourceGenerator", true)]
    [InlineData("__Internal", "__Internal", true)]
    [InlineData("__Internal", "__internal", false)]
    public void EqualsNamespaceString_ReturnsEquivalenceWithCaseSensitivity(string a, string b, bool expected)
    {
        new NamespaceString(a).Equals(new NamespaceString(b)).Should().Be(expected);
    }

    [Theory]
    [InlineData("Reqnroll", "Reqnroll", true)]
    [InlineData("Reqnroll", "reqnroll", false)]
    [InlineData("Reqnroll.FeatureSourceGenerator", "Reqnroll.FeatureSourceGenerator", true)]
    [InlineData("__Internal", "__Internal", true)]
    [InlineData("__Internal", "__internal", false)]
    [InlineData("__Internal", "1234", false)]
    [InlineData("__Internal", ".Reqnroll", false)]
    public void EqualsString_ReturnsEquivalenceWithCaseSensitivity(string a, string b, bool expected)
    {
        new NamespaceString(a).Equals(b).Should().Be(expected);
    }

    [Theory]
    [InlineData("Reqnoll")]
    [InlineData("Reqnoll.FeatureSourceGenerator")]
    [InlineData("__Internal")]
    [InlineData("_1XYZ")]
    [InlineData("Reqnroll.__Internal")]
    [InlineData("")]
    [InlineData(null)]
    public void GetHashCode_ReturnsSameValueForEquivalentValues(string s)
    {
        new NamespaceString(s).GetHashCode().Should().Be(new NamespaceString(s).GetHashCode());
    }

    [Theory]
    [InlineData("Reqnroll", "Reqnroll", true)]
    [InlineData("Reqnroll", "reqnroll", false)]
    [InlineData("Reqnroll.FeatureSourceGenerator", "Reqnroll.FeatureSourceGenerator", true)]
    [InlineData("__Internal", "__Internal", true)]
    [InlineData("__Internal", "__internal", false)]
    public void EqualityOperatorWithString_ReturnsEquivalenceWithCaseSensitivity(string a, string b, bool expected)
    {
        (new NamespaceString(a) == new NamespaceString(b)).Should().Be(expected);
    }

    [Theory]
    [InlineData("Reqnroll", "Reqnroll", true)]
    [InlineData("Reqnroll", "reqnroll", false)]
    [InlineData("Reqnroll.FeatureSourceGenerator", "Reqnroll.FeatureSourceGenerator", true)]
    [InlineData("__Internal", "__Internal", true)]
    [InlineData("__Internal", "__internal", false)]
    public void EqualityOperatorWithNamespaceString_ReturnsEquivalenceWithCaseSensitivity(string a, string b, bool expected)
    {
        (new NamespaceString(a) == b).Should().Be(expected);
    }

    [Theory]
    [InlineData("Reqnroll", "Reqnroll", false)]
    [InlineData("Reqnroll", "reqnroll", true)]
    [InlineData("Reqnroll.FeatureSourceGenerator", "Reqnroll.FeatureSourceGenerator", false)]
    [InlineData("__Internal", "__Internal", false)]
    [InlineData("__Internal", "__internal", true)]
    public void InequalityOperatorWithString_ReturnsNonEquivalenceWithCaseSensitivity(string a, string b, bool expected)
    {
        (new NamespaceString(a) != new NamespaceString(b)).Should().Be(expected);
    }

    [Theory]
    [InlineData("Reqnroll", "Reqnroll", false)]
    [InlineData("Reqnroll", "reqnroll", true)]
    [InlineData("Reqnroll.FeatureSourceGenerator", "Reqnroll.FeatureSourceGenerator", false)]
    [InlineData("__Internal", "__Internal", false)]
    [InlineData("__Internal", "__internal", true)]
    public void InequalityOperatorWithNamespaceString_ReturnsNonEquivalenceWithCaseSensitivity(string a, string b, bool expected)
    {
        (new NamespaceString(a) != b).Should().Be(expected);
    }
}
