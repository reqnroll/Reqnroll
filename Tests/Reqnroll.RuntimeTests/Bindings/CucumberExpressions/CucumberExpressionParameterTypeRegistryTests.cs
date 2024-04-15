using System;
using System.Linq;
using FluentAssertions;
using Reqnroll.Bindings;
using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Bindings.Reflection;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings.CucumberExpressions {

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

        public class SampleEnumUsingClass
        {
            public void MethodUsingSampleColorEnum1(SampleColorEnum color) { }
        }
        [Fact]
        public void Should_not_error_on_multiple_enums_of_the_same_name()
        {
            var sut = CreateSut();
            Reqnroll.Bindings.Reflection.IBindingMethod enumUsingBindingMethod1 = new RuntimeBindingMethod(typeof(SampleEnumUsingClass).GetMethod(nameof(SampleEnumUsingClass.MethodUsingSampleColorEnum1)));
            sut.OnBindingMethodProcessed(enumUsingBindingMethod1);
            Reqnroll.Bindings.Reflection.IBindingMethod enumUsingBindingMethod2 = new RuntimeBindingMethod(typeof(CucumberAddtionalExpressions.EnumCucumberExpressions).GetMethod(nameof(CucumberAddtionalExpressions.EnumCucumberExpressions.MethodUsingSampleColorEnum2)));
            sut.OnBindingMethodProcessed(enumUsingBindingMethod2);
            var paramTypes = sut.GetParameterTypes().Where(pt => pt.ParameterType.IsEnum).ToList();
                
            paramTypes.Should().HaveCount(2);
        }

    }
}
namespace Reqnroll.RuntimeTests.Bindings.CucumberAddtionalExpressions
{
    public class EnumCucumberExpressions
    {
        public enum SampleColorEnum { Yellow, Brown };
 
        public void MethodUsingSampleColorEnum2(SampleColorEnum color) { }
    }
}