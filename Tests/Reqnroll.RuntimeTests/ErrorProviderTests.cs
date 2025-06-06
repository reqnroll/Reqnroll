using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.ErrorHandling;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Configuration;
using Xunit;

namespace Reqnroll.RuntimeTests
{
    public class ErrorProviderTests
    {
        private static ErrorProvider CreateErrorProvider(IStepFormatter stepFormatter = null, Reqnroll.Configuration.ReqnrollConfiguration reqnrollConfiguration = null, IUnitTestRuntimeProvider unitTestRuntimeProvider = null)
        {
            return new ErrorProvider(stepFormatter, reqnrollConfiguration, unitTestRuntimeProvider);
        }

        private IBindingMethod CreateBindingMethod(string methodName, string methodBindingTypeName, string methodBindingTypeFullName, string methodBindingAssemblyName, params string[] parametersTypes)
        {
            parametersTypes = parametersTypes ?? new string[0];
            return new BindingMethod(
                new BindingType(methodBindingTypeName, methodBindingTypeFullName, methodBindingAssemblyName),
                methodName,
                parametersTypes.Select(pn => new BindingParameter(new BindingType(pn, pn, string.Empty), string.Empty)),
                null);
        }

        [Fact]
        public void GetMethodText_should_return_string_containing_full_assembly_name_method_name_and_parameters_types()
        {
            const string methodName = "WhenIAdd";
            const string methodBindingTypeName = "CalculatorSteps";
            const string methodBindingAssemblyName = "StepsAssembly1";
            const string methodBindingTypeFullName = "StepsNamespace.CalculatorSteps";
            const string parameter1Type = "Int32";
            const string parameter2Type = "String";

            var errorProvider = CreateErrorProvider();

            var bindingMethod = CreateBindingMethod(methodName, methodBindingTypeName, methodBindingTypeFullName, methodBindingAssemblyName, parameter1Type, parameter2Type);

            var result = errorProvider.GetMethodText(bindingMethod);

            result.Should().NotBeNull();
            result.Should().Be($"{methodBindingAssemblyName}:{methodBindingTypeFullName}.{methodName}({parameter1Type}, {parameter2Type})");
        }

        [Fact]
        public void GetCallError_should_return_BindingException_containing_full_assembly_name_method_name_and_parameters_types_and_exception_message()
        {
            const string methodName = "WhenIMultiply";
            const string methodBindingTypeName = "CalculatorSteps";
            const string methodBindingAssemblyName = "StepsAssembly1";
            const string methodBindingTypeFullName = "StepsNamespace.CalculatorSteps";
            const string parameter1Type = "String";
            const string parameter2Type = "Int64";

            const string expectedExceptionMessage = "Initialization failed";

            var errorProvider = CreateErrorProvider();

            var bindingMethod = CreateBindingMethod(methodName, methodBindingTypeName, methodBindingTypeFullName, methodBindingAssemblyName, parameter1Type, parameter2Type);

            var exceptionStub = new Mock<Exception>();
            exceptionStub.Setup(e => e.Message).Returns(expectedExceptionMessage);

            var result = errorProvider.GetCallError(bindingMethod, exceptionStub.Object);

            result.Should().NotBeNull();
            result.Should().BeOfType<BindingException>();
            result.Message.Should().Be($"Error calling binding method '{methodBindingAssemblyName}:{methodBindingTypeFullName}.{methodName}({parameter1Type}, {parameter2Type})': {expectedExceptionMessage}");
            result.InnerException.Should().BeSameAs(exceptionStub.Object);
        }

        [Fact]
        public void GetParameterCountError_should_return_BindingException_containing_full_assembly_name_method_name_and_parameters_types_and_expected_parameter_count()
        {
            const string methodName = "WhenIMultiply";
            const string methodBindingTypeName = "CalculatorSteps";
            const string methodBindingAssemblyName = "StepsAssembly1";
            const string methodBindingTypeFullName = "StepsNamespace.CalculatorSteps";
            const string parameter1Type = "Int64";

            var errorProvider = CreateErrorProvider();

            var bindingMethod = CreateBindingMethod(methodName, methodBindingTypeName, methodBindingTypeFullName, methodBindingAssemblyName, parameter1Type);

            var stepDefinitionStub = new Mock<IStepDefinitionBinding>();
            stepDefinitionStub.Setup(sd => sd.Method).Returns(bindingMethod);
            var result = errorProvider.GetParameterCountError(new BindingMatch(stepDefinitionStub.Object, It.IsAny<int>(), null, null), 2);

            result.Should().NotBeNull();
            result.Should().BeOfType<BindingException>();
            result.Message.Should().Be($"Parameter count mismatch! The binding method '{methodBindingAssemblyName}:{methodBindingTypeFullName}.{methodName}({parameter1Type})' should have 2 parameters");
        }



        private void GetMatchErrorMethod_should_return_BindingException_containing_full_assembly_names_method_names_parameters_types_and_step_description(Func<ErrorProvider, List<BindingMatch>, StepInstance, Exception> GetMatchErrorFunc, string expectedPrefixMessage)
        {
            const string methodName = "WhenIMultiply";
            const string methodBindingTypeName = "CalculatorSteps";
            const string methodBindingAssemblyName = "StepsAssembly1";
            const string method1BindingTypeFullName = "StepsNamespace.CalculatorSteps";
            const string method2BindingTypeFullName = "StepsNamespace.CalculatorSteps";
            const string parameter1Type = "Int64";

            const string stepInstanceDescription = "'Given I multiply 10 and 5'";

            var stepFormatterStub = new Mock<IStepFormatter>();
            stepFormatterStub.Setup(f => f.GetStepDescription(It.IsAny<StepInstance>())).Returns(stepInstanceDescription);
            var errorProvider = CreateErrorProvider(stepFormatterStub.Object);

            var bindingMethod1 = CreateBindingMethod(methodName, methodBindingTypeName, method1BindingTypeFullName, methodBindingAssemblyName, parameter1Type);
            var bindingMethod2 = CreateBindingMethod(methodName, methodBindingTypeName, method2BindingTypeFullName, methodBindingAssemblyName, parameter1Type);

            var stepDefinitionStub1 = new Mock<IStepDefinitionBinding>();
            stepDefinitionStub1.Setup(sd => sd.Method).Returns(bindingMethod1);
            var stepDefinitionStub2 = new Mock<IStepDefinitionBinding>();
            stepDefinitionStub2.Setup(sd => sd.Method).Returns(bindingMethod2);
            var bindingMatch = new List<BindingMatch>()
            {
                new BindingMatch(stepDefinitionStub1.Object, It.IsAny<int>(), null, null),
                new BindingMatch(stepDefinitionStub2.Object, It.IsAny<int>(), null, null)
            };

            var result = GetMatchErrorFunc(errorProvider, bindingMatch, null);

            result.Should().NotBeNull();
            result.Should().BeAssignableTo<BindingException>();
            result.Message.Should().Be($"{expectedPrefixMessage} '{stepInstanceDescription}': {methodBindingAssemblyName}:{method1BindingTypeFullName}.{methodName}({parameter1Type}), {methodBindingAssemblyName}:{method2BindingTypeFullName}.{methodName}({parameter1Type})");
        }

        [Fact]
        public void GetAmbiguousMatchError_should_return_BindingException_containing_full_assembly_names_method_names_parameters_types_and_step_description()
        {
            const string prefixMessage = "Ambiguous step definitions found for step";
            GetMatchErrorMethod_should_return_BindingException_containing_full_assembly_names_method_names_parameters_types_and_step_description(
                (errorProvider, matches, stepInstance) => errorProvider.GetAmbiguousMatchError(matches, stepInstance), prefixMessage);
        }

        [Fact]
        public void GetAmbiguousBecauseParamCheckMatchError_should_return_BindingException_containing_full_assembly_names_method_names_parameters_types_and_step_description()
        {
            const string prefixMessage = "Multiple step definitions found, but none of them have matching parameter count and type for step";
            GetMatchErrorMethod_should_return_BindingException_containing_full_assembly_names_method_names_parameters_types_and_step_description(
                (errorProvider, matches, stepInstance) => errorProvider.GetAmbiguousBecauseParamCheckMatchError(matches, stepInstance), prefixMessage);
        }

        [Fact]
        public void GetNoMatchBecauseOfScopeFilterError_should_return_BindingException_containing_full_assembly_names_method_names_parameters_types_and_step_description()
        {
            const string prefixMessage = "Multiple step definitions found, but none of them have matching scope for step";
            GetMatchErrorMethod_should_return_BindingException_containing_full_assembly_names_method_names_parameters_types_and_step_description(
                (errorProvider, matches, stepInstance) => errorProvider.GetNoMatchBecauseOfScopeFilterError(matches, stepInstance), prefixMessage);
        }

        [Fact]
        public void GetMissingStepDefinitionError_Throws_MissingStepDefinitionException()
        {
            var errorProvider = CreateErrorProvider();

            var result = errorProvider.GetMissingStepDefinitionError();

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetPendingStepDefinitionError_Throws_MissingStepDefinitionException()
        {
            var errorProvider = CreateErrorProvider();

            var result = errorProvider.GetPendingStepDefinitionError();

            result.Should().NotBeNull();
        }

        private static Mock<IUnitTestRuntimeProvider> ThrowPendingError(MissingOrPendingStepsOutcome missingOrPendingStepsOutcome, string expectedMessage, ScenarioExecutionStatus scenarioExecutionStatus = ScenarioExecutionStatus.UndefinedStep)
        {
            var reqnrollConfiguration = ConfigurationLoader.GetDefault();
            reqnrollConfiguration.MissingOrPendingStepsOutcome = missingOrPendingStepsOutcome;
            var testRuntimeProviderMock = new Mock<IUnitTestRuntimeProvider>();
            var errorProvider = CreateErrorProvider(null, reqnrollConfiguration, testRuntimeProviderMock.Object);

            errorProvider.ThrowPendingError(scenarioExecutionStatus, expectedMessage);

            return testRuntimeProviderMock;
        }

        [Fact]
        public void ThrowPendingError_Signals_TestPending_With_Message_To_Test_Provider_When_StepsOutcome_Is_Pending()
        {
            const string expectedMessage = "Expected message";
            var missingOrPendingStepsOutcome = MissingOrPendingStepsOutcome.Pending;

            var testRuntimeProviderMock = ThrowPendingError(missingOrPendingStepsOutcome, expectedMessage);

            testRuntimeProviderMock.Verify(p => p.TestPending(expectedMessage));
        }

        [Fact]
        public void ThrowPendingError_Signals_TestInconclusive_With_Message_To_Test_Provider_When_StepsOutcome_Is_Inconclusive()
        {
            const string expectedMessage = "Expected message";
            var missingOrPendingStepsOutcome = MissingOrPendingStepsOutcome.Inconclusive;

            var testRuntimeProviderMock = ThrowPendingError(missingOrPendingStepsOutcome, expectedMessage);

            testRuntimeProviderMock.Verify(p => p.TestInconclusive(expectedMessage));
        }

        [Fact]
        public void ThrowPendingError_Signals_TestIgnore_With_Message_To_Test_Provider_When_StepsOutcome_Is_Ignore()
        {
            const string expectedMessage = "Expected message";
            var missingOrPendingStepsOutcome = MissingOrPendingStepsOutcome.Ignore;

            var testRuntimeProviderMock = ThrowPendingError(missingOrPendingStepsOutcome, expectedMessage);

            testRuntimeProviderMock.Verify(p => p.TestIgnore(expectedMessage));
        }

        [Fact]
        public void ThrowPendingError_Throws_MissingStepDefinitionException_When_StepsOutcome_Is_Error_And_Test_Status_Is_UndefinedStep()
        {
            string unusedMessage = "";

            var missingOrPendingStepsOutcome = MissingOrPendingStepsOutcome.Error;

            Assert.Throws<MissingStepDefinitionException>(() => ThrowPendingError(missingOrPendingStepsOutcome, unusedMessage, ScenarioExecutionStatus.UndefinedStep));
        }

        [Fact]
        public void ThrowPendingError_Throws_PendingStepException_When_StepsOutcome_Is_Error_And_Test_Status_Is_Error()
        {
            string unusedMessage = "";

            var missingOrPendingStepsOutcome = MissingOrPendingStepsOutcome.Error;

            Assert.Throws<PendingStepException>(() => ThrowPendingError(missingOrPendingStepsOutcome, unusedMessage, ScenarioExecutionStatus.TestError));
        }

        [Fact]
        public void GetTooManyBindingParamError_Returns_BindingException_With_message_containing_Max_Number_Of_Bindings()
        {
            const int maxParam = 5;

            var errorProvider = CreateErrorProvider();

            var result = errorProvider.GetTooManyBindingParamError(maxParam);

            result.Should().NotBeNull();
            result.Should().BeOfType<BindingException>();
            result.Message.Should().Be($"Binding methods with more than {maxParam} parameters are not supported");
        }

        [Fact]
        public void GetInvalidBindingRegistryError_Throws_BindingException_with_message_containing_all_errors()
        {
            var errorProvider = CreateErrorProvider();

            Assert.Throws<BindingException>(() => errorProvider.GetInvalidBindingRegistryError(new[] { new BindingError(BindingErrorType.BindingError, "error1"), new BindingError(BindingErrorType.BindingError, "error2") }));
        }
    }
}