using Reqnroll.Bindings.Provider;
using System.Reflection;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Reqnroll.RuntimeTests.Bindings.Provider;
public class BindingProviderServiceTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Should_provide_step_definitions()
    {
        var result = BindingProviderService.DiscoverBindings(Assembly.GetExecutingAssembly(), null);
        testOutputHelper.WriteLine(result);
        result.Should().NotBeNull();
        result.Should().Contain("\"StepDefinitions\":");
    }
}
