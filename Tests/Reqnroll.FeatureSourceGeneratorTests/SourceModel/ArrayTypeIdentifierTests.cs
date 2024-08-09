namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public class ArrayTypeIdentifierTests
{
    [Theory]
    [InlineData("Parser", true, false)]
    [InlineData("__Parser", false, false)]
    [InlineData("X509", true, true)]
    public void Constructor_CreatesArrayTypeFromType(string localName, bool itemIsNullable, bool arrayIsNullable)
    {
        var itemType = new SimpleTypeIdentifier(new IdentifierString(localName), itemIsNullable);

        var identifier = new ArrayTypeIdentifier(itemType, arrayIsNullable);

        identifier.ItemType.Should().Be(itemType);
        identifier.IsNullable.Should().Be(arrayIsNullable);
    }

    [Theory]
    [InlineData("Parser", true, "Parser", true, true)]
    [InlineData("_Parser", false, "_Parser", false, true)]
    [InlineData("Parser", false, "Parser", true, false)]
    [InlineData("Parser", true, "_Parser", false, false)]
    public void Equals_ReturnsTrueWhenItemTypeAndNullabilityMatches(
        string nameA,
        bool arrayAIsNullable,
        string nameB,
        bool arrayBIsNullable,
        bool expected)
    {
        var typeIdA = new SimpleTypeIdentifier(new IdentifierString(nameA));
        var typeIdB = new SimpleTypeIdentifier(new IdentifierString(nameB));

        var arrayTypeA = new ArrayTypeIdentifier(typeIdA, arrayAIsNullable);
        var arrayTypeB = new ArrayTypeIdentifier(typeIdB, arrayBIsNullable);

        arrayTypeA.Equals(arrayTypeB).Should().Be(expected);
    }

    [Theory]
    [InlineData("Parser", true, "Parser", true)]
    [InlineData("_Parser", false, "_Parser", false)]
    public void GetHashCode_ReturnsSameValueForEquivalentValues(
        string nameA,
        bool arrayAIsNullable,
        string nameB,
        bool arrayBIsNullable)
    {
        var typeIdA = new SimpleTypeIdentifier(new IdentifierString(nameA));
        var typeIdB = new SimpleTypeIdentifier(new IdentifierString(nameB));

        var arrayTypeA = new ArrayTypeIdentifier(typeIdA, arrayAIsNullable);
        var arrayTypeB = new ArrayTypeIdentifier(typeIdB, arrayBIsNullable);

        arrayTypeA.GetHashCode().Should().Be(arrayTypeB.GetHashCode());
    }

    [Theory]
    [InlineData("Parser", true, "Parser", true, true)]
    [InlineData("_Parser", false, "_Parser", false, true)]
    [InlineData("Parser", false, "Parser", true, false)]
    [InlineData("Parser", true, "_Parser", false, false)]
    public void EqualityOperatorWithArrayTypeIdentifier_ReturnsEquivalenceBasedOnItemTypeAndNullability(
        string nameA,
        bool arrayAIsNullable,
        string nameB,
        bool arrayBIsNullable,
        bool expected)
    {
        var typeIdA = new SimpleTypeIdentifier(new IdentifierString(nameA));
        var typeIdB = new SimpleTypeIdentifier(new IdentifierString(nameB));

        var arrayTypeA = new ArrayTypeIdentifier(typeIdA, arrayAIsNullable);
        var arrayTypeB = new ArrayTypeIdentifier(typeIdB, arrayBIsNullable);

        (arrayTypeA == arrayTypeB).Should().Be(expected);
    }

    [Theory]
    [InlineData("Parser", true, "Parser", true, false)]
    [InlineData("_Parser", false, "_Parser", false, false)]
    [InlineData("Parser", false, "Parser", true, true)]
    [InlineData("Parser", true, "_Parser", false, true)]
    public void InequalityOperatorWithArrayTypeIdentifier_ReturnsNonEquivalenceBasedOnItemTypeAndNullability(
        string nameA,
        bool arrayAIsNullable,
        string nameB,
        bool arrayBIsNullable,
        bool expected)
    {
        var typeIdA = new SimpleTypeIdentifier(new IdentifierString(nameA));
        var typeIdB = new SimpleTypeIdentifier(new IdentifierString(nameB));

        var arrayTypeA = new ArrayTypeIdentifier(typeIdA, arrayAIsNullable);
        var arrayTypeB = new ArrayTypeIdentifier(typeIdB, arrayBIsNullable);

        (arrayTypeA != arrayTypeB).Should().Be(expected);
    }
}
