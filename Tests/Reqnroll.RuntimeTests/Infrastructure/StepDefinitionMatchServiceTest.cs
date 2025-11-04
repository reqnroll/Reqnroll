using System.Collections.Generic;
using System.Globalization;
using Moq;
using Xunit;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Infrastructure;
using FluentAssertions;
using Reqnroll.RuntimeTests.ErrorHandling;
using Reqnroll.Bindings.Discovery;
using System;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    
    public class StepDefinitionMatchServiceTest
    {
        private Mock<IBindingRegistry> bindingRegistryMock;
        private Mock<IStepArgumentTypeConverter> stepArgumentTypeConverterMock;
        private readonly CultureInfo bindingCulture = new CultureInfo("en-US", false);
        private List<IStepDefinitionBinding> whenStepDefinitions;
            
        public StepDefinitionMatchServiceTest()
        {
            whenStepDefinitions = new List<IStepDefinitionBinding>();
            bindingRegistryMock = new Mock<IBindingRegistry>();
            bindingRegistryMock.Setup(r => r.GetConsideredStepDefinitions(StepDefinitionType.When, It.IsAny<string>()))
                .Returns(whenStepDefinitions);

            stepArgumentTypeConverterMock = new Mock<IStepArgumentTypeConverter>();
        }

        private StepDefinitionMatchService CreateSUT()
        {
            return new StepDefinitionMatchService(bindingRegistryMock.Object, stepArgumentTypeConverterMock.Object, new StubErrorProvider(), new MatchArgumentCalculator());
        }

        private static BindingMethod CreateBindingMethod(string name = "dummy")
        {
            return new BindingMethod(new BindingType("dummy", "dummy", "dummy"), name, new IBindingParameter[0], null);
        }

        private static BindingMethod CreateBindingMethodWithStringParam(string name = "dummy")
        {
            return new BindingMethod(new BindingType("dummy", "dummy", "dummy"), name, new IBindingParameter[] { new BindingParameter(new RuntimeBindingType(typeof(string)), "param1") }, null);
        }

        private static BindingMethod CreateBindingMethodWithDataTableParam(string name = "dummy")
        {
            return new BindingMethod(new BindingType("dummy", "dummy", "dummy"), name, new IBindingParameter[] { new BindingParameter(new RuntimeBindingType(typeof(Table)), "param1") }, null);
        }

        private static BindingMethod CreateBindingMethodWithObjectParam(string name = "dummy")
        {
            return new BindingMethod(new BindingType("dummy", "dummy", "dummy"), name, new IBindingParameter[] { new BindingParameter(new RuntimeBindingType(typeof(object)), "param1") }, null);
        }

        private StepInstance CreateSimpleWhen(string text = "I do something", IEnumerable<string> tags = null)
        {
            tags = tags ?? new string[0];
            var result = new StepInstance(StepDefinitionType.When, StepDefinitionKeyword.When, "When ", text, null, null, 
                new StepContext("MyFeature", "MyScenario", tags, new CultureInfo("en-US", false)));
            return result;
        }

        private BindingScope CreateBindingScope(string tag, string featureTitle, string scenarioTitle)
        {
            var tagExpressionParser = new ReqnrollTagExpressionParser(new Cucumber.TagExpressions.TagExpressionParser());
            return new BindingScope(tagExpressionParser.Parse(tag), featureTitle, scenarioTitle);
        }

        [Fact]
        public void Should_GetBestMatch_succeed_when_proper_match()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod()));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out _, out _);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Should_GetBestMatch_succeed_when_proper_match_and_non_matching_scopes()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod("m1")));
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod("m2"), CreateBindingScope("non-matching-tag", null, null)));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out _, out _);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Should_GetBestMatch_succeed_when_proper_match_and_matching_tag_expression()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod("m1")));
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod("m2"), CreateBindingScope("@alpha and @beta", null, null)));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen("I do something", new string[] { "alpha", "beta" }), bindingCulture, out _, out _);

            result.Success.Should().BeTrue();
            result.StepBinding.Method.Name.Should().Be("m2");
        }



        [Fact]
        public void Should_GetBestMatch_succeed_when_proper_match_with_parameters()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, "(.*)", CreateBindingMethodWithStringParam()));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out _, out _);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Should_GetBestMatch_succeed_when_proper_match_with_parameters_even_if_there_is_a_DataTable_overload()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, "(.*)", CreateBindingMethodWithStringParam()));
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethodWithDataTableParam()));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out _, out _);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Should_GetBestMatch_succeed_when_proper_match_with_object_parameters()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, "(.*)", CreateBindingMethodWithObjectParam()));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out _, out _);

            result.Success.Should().BeTrue();
        }


        [Fact]
        public void Should_GetBestMatch_succeed_when_proper_match_with_object_parameters_even_if_there_is_a_DataTable_overload()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, "(.*)", CreateBindingMethodWithObjectParam()));
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethodWithDataTableParam()));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out _, out _);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Should_GetBestMatch_fail_when_scope_errors_with_single_match()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod(), CreateBindingScope("non-matching-tag", null, null)));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out var ambiguityReason, out _);

            result.Success.Should().BeFalse();
            ambiguityReason.Should().Be(StepDefinitionAmbiguityReason.AmbiguousScopes);
        }

        [Fact]
        public void Should_GetBestMatch_fail_when_scope_errors_with_multiple_matches()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod("dummy1"), CreateBindingScope("non-matching-tag", null, null)));
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod("dummy2"), CreateBindingScope("other-non-matching-tag", null, null)));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out var ambiguityReason, out _);

            result.Success.Should().BeFalse();
            ambiguityReason.Should().Be(StepDefinitionAmbiguityReason.AmbiguousScopes);
        }

        [Fact] // in case of single parameter error, we pretend success - the error will be displayed runtime
        public void Should_GetBestMatch_succeed_when_parameter_errors_with_single_match()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, "(.*)", CreateBindingMethod()));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out _, out _);

            result.Success.Should().BeTrue(); 
        }

        [Fact]
        public void Should_GetBestMatch_fail_when_parameter_errors_with_multiple_matches()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, "(.*)", CreateBindingMethod("dummy1")));
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, "(.*)", CreateBindingMethod("dummy2")));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out var ambiguityReason, out _);

            result.Success.Should().BeFalse();
            ambiguityReason.Should().Be(StepDefinitionAmbiguityReason.ParameterErrors);
        }

        [Fact]
        public void Should_GetBestMatch_fail_when_multiple_matches()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod("dummy1")));
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod("dummy2")));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out var ambiguityReason, out _);

            result.Success.Should().BeFalse();
            ambiguityReason.Should().Be(StepDefinitionAmbiguityReason.AmbiguousSteps);
        }

        [Fact]
        public void Should_GetBestMatch_succeed_when_multiple_matches_are_on_the_same_method()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod()));
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, ".*", CreateBindingMethod()));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out _, out _);

            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Should_GetBestMatch_succeed_when_no_matching_step_definitions()
        {
            whenStepDefinitions.Add(StepDefinitionHelper.CreateRegex(StepDefinitionType.When, "non-maching-regex", CreateBindingMethod()));

            var sut = CreateSUT();

            var result = sut.GetBestMatch(CreateSimpleWhen(), bindingCulture, out var ambiguityReason, out _);

            result.Success.Should().BeFalse();
            ambiguityReason.Should().Be(StepDefinitionAmbiguityReason.None);
        }
    }
}
