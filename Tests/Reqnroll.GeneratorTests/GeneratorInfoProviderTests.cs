using FluentAssertions;
using Reqnroll.Generator.Configuration;
using Xunit;

namespace Reqnroll.GeneratorTests;

public class GeneratorInfoProviderTests : TestGeneratorTestsBase
{
    [Fact]
    public void GetGeneratorVersion_should_return_a_version()
    {
        var sut = new GeneratorInfoProvider();

        sut.GetGeneratorInfo().GeneratorVersion.Should().NotBeNull();
    }
}