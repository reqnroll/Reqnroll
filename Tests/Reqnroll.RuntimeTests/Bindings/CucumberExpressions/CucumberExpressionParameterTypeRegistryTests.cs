using System;
using FluentAssertions;
using Reqnroll.Bindings;
using Reqnroll.Bindings.CucumberExpressions;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings.CucumberExpressions;

public class CucumberExpressionParameterTypeRegistryTests
{
    // Most of the logic in CucumberExpressionParameterTypeRegistry can only be tested in integration of a cucumber expression match,
    // so those are tested in the CucumberExpressionIntegrationTests class.

    private BindingRegistry _bindingRegistry;
    private CucumberExpressionParameterTypeRegistry CreateSut()
    {
        _bindingRegistry = new BindingRegistry();
        return new CucumberExpressionParameterTypeRegistry(_bindingRegistry);
    }

    [Fact]
    public void Should_provide_string_type()
    {
        var sut = CreateSut();
        var paramType = sut.LookupByTypeName("string");

        // The regex '.*' provided by the CucumberExpressionParameterTypeRegistry is fake and
        // will be ignored because of the special string type handling implemented in ReqnrollCucumberExpression.
        // See ReqnrollCucumberExpression.HandleStringType for detailed explanation.
        paramType.Should().NotBeNull();
        paramType.RegexStrings.Should().HaveCount(1);
        paramType.RegexStrings[0].Should().Be(".*");
    }
}