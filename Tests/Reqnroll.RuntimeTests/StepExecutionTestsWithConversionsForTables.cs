using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.RuntimeTests
{
    [Binding]
    public class StepExecutionTestsBindingsForTableArgumentConvert
    {

        [Given("sample step for argument convert with table")]
        public virtual void SingleTable(User table)
        {

        }


        [Given("sample step for argument convert with multiline argument and table")]
        public virtual void MultilineArgumentAndTable(string multilineArg, User table)
        {

        }

        [Given("sample step for argument convert with parameter, multiline argument and table: (.*)")]
        public virtual void ParameterMultilineArgumentAndTable(string param, string multilineArg, User table)
        {

        }
    }

    
    public class StepExecutionTestsWithConversionsForTables : StepExecutionTestsBase
    {
        [Fact]
        public async Task ShouldCallTheUserConverterToConvertTableWithTable()
        {
            var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForTableArgumentConvert>();

            Table table = new Table("h1");
            var user = new User();

            // return false unless it's a User
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert(table, ArgumentHelpers.IsBindingType<User>(), It.IsAny<CultureInfo>())).Returns(true);
            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync(It.Is<object>(s => s.Equals(table)), ArgumentHelpers.IsBindingType<User>(), It.IsAny<CultureInfo>())).ReturnsAsync(user);

            await testRunner.GivenAsync("sample step for argument convert with table", null, table);

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK);
            bindingMock.Verify(x => x.SingleTable(user));
        }

        [Fact]
        public async Task ShouldCallTheUserConverterToConvertTableWithTableAndMultilineArg()
        {
            var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForTableArgumentConvert>();

            Table table = new Table("h1");
            var user = new User();
            var multiLineArg = "multi-line arg";
            // return false unless it's a User
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert(table, ArgumentHelpers.IsBindingType<User>(), It.IsAny<CultureInfo>())).Returns(true);            
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert(It.IsAny<object>(), It.IsAny<IBindingType>(), It.IsAny<CultureInfo>())).Returns(false);
            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync(multiLineArg, ArgumentHelpers.IsBindingType<string>(), It.IsAny<CultureInfo>())).ReturnsAsync(multiLineArg);
            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync(It.Is<object>(s => s.Equals(table)), ArgumentHelpers.IsBindingType<User>(), It.IsAny<CultureInfo>())).ReturnsAsync(user);
            
            await testRunner.GivenAsync("sample step for argument convert with multiline argument and table", multiLineArg, table);

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK);
            bindingMock.Verify(x => x.MultilineArgumentAndTable(multiLineArg, user));
        }

        [Fact]
        public async Task ShouldCallTheUserConverterToConvertTableWithTableAndMultilineArgAndParameter()
        {
            var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForTableArgumentConvert>();

            Table table = new Table("h1");
            string argumentValue = "argument";
            var user = new User();
            var multiLineArg = "multi-line arg";
            // return false unless it's a User
            // must also stub CanConvert & Convert for the string argument as we've introduced a parameter
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert(table, ArgumentHelpers.IsBindingType<User>(), It.IsAny<CultureInfo>())).Returns(true);
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert(It.IsAny<object>(), It.IsAny<IBindingType>(), It.IsAny<CultureInfo>())).Returns(false);
            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync(It.Is<object>(s => s.Equals(table)), ArgumentHelpers.IsBindingType<User>(), It.IsAny<CultureInfo>())).ReturnsAsync(user);
            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync(argumentValue, ArgumentHelpers.IsBindingType<string>(), It.IsAny<CultureInfo>())).ReturnsAsync(argumentValue);
            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync(multiLineArg, ArgumentHelpers.IsBindingType<string>(), It.IsAny<CultureInfo>())).ReturnsAsync(multiLineArg);

            await testRunner.GivenAsync("sample step for argument convert with parameter, multiline argument and table: argument", multiLineArg, table);

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK);
            bindingMock.Verify(x => x.ParameterMultilineArgumentAndTable(argumentValue, multiLineArg, user));
        }
    }
}