using FluentAssertions;
using Reqnroll.Generator.Configuration;
using Xunit;

namespace Reqnroll.GeneratorTests;

public class GeneratorInfoProviderTests : TestGeneratorTestsBase
{
    [Fact]
    public void GetGeneratorVersion_should_return_a_version()
    {
        GeneratorInfoProvider.GetGeneratorInfo().GeneratorVersion.Should().NotBeNull();
    }
}