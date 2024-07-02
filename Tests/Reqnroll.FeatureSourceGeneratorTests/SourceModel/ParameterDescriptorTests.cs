using Reqnroll.FeatureSourceGenerator;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;
public class ParameterDescriptorTests
{
    [Fact]
    public void Constructor_ThrowsArgumentExceptionWhenNameIsEmpty()
    {
        Func<ParameterDescriptor> ctr = () =>
            new ParameterDescriptor(IdentifierString.Empty, new NamedTypeIdentifier(new IdentifierString("string")));

        ctr.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var name = new IdentifierString("potato");
        var type = new NamedTypeIdentifier(new IdentifierString("string"));

        var descriptor = new ParameterDescriptor(name, type);

        descriptor.Name.Should().Be(name);
        descriptor.Type.Should().Be(type);
    }

    [Theory]
    [InlineData("potato", "System", "String")]
    [InlineData("foo", "System", "Int32")]
    [InlineData("bar", "System", "Int64")]
    [InlineData("fiz", "", "Buzz")]
    public void GetHashCode_ReturnsSameValueForEquivalentValues(string name, string typeNamespace, string typeName)
    {
        var descriptorA = new ParameterDescriptor(
            new IdentifierString(name),
            new NamedTypeIdentifier(new NamespaceString(typeNamespace), new IdentifierString(typeName)));

        var descriptorB = new ParameterDescriptor(
            new IdentifierString(name),
            new NamedTypeIdentifier(new NamespaceString(typeNamespace), new IdentifierString(typeName)));

        descriptorA.GetHashCode().Should().Be(descriptorB.GetHashCode());
    }

    [Theory]
    [InlineData("potato", "System", "String", "potato", "System", "String", true)]
    [InlineData("potato", "System", "String", "potato", "", "String", false)]
    [InlineData("potato", "System", "String", "foo", "System", "String", false)]
    [InlineData("potato", "System", "String", "foo", "", "String", false)]
    public void Equals_ReturnsEqualityBasedOnMatchingPropertyValues(
        string nameA,
        string typeNamespaceA,
        string typeNameA,
        string nameB,
        string typeNamespaceB,
        string typeNameB,
        bool expected)
    {
        var descriptorA = new ParameterDescriptor(
            new IdentifierString(nameA),
            new NamedTypeIdentifier(new NamespaceString(typeNamespaceA), new IdentifierString(typeNameA)));

        var descriptorB = new ParameterDescriptor(
            new IdentifierString(nameB),
            new NamedTypeIdentifier(new NamespaceString(typeNamespaceB), new IdentifierString(typeNameB)));

        descriptorA.Equals(descriptorB).Should().Be(expected);
    }
}
