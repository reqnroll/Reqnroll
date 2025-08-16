using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Moq;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.RuntimeTests
{
    [Binding]
    public class StepExecutionTestsBindingsForArgumentConvert
    {
        [Given("sample step for argument convert: (.*)")]
        public virtual void IntArg(int param)
        {

        }

        [Given("sample step for argument convert: (.*)")]
        public virtual void DoubleArg(double param)
        {

        }

        [Given("sample step for argument convert with table: (.*)")]
        public virtual void IntArgWithTable(int param, Table table)
        {

        }

        [Given("sample step for argument convert with table: (.*)")]
        public virtual void DoubleArgWithTable(double param, Table table)
        {

        }
    }

    public class StepExecutionTestsWithConversions : StepExecutionTestsBase
    {
        [Fact]
        public async Task ShouldCallBindingWithSimpleConvertParam()
        {
            var (testRunner, bindingMock) = GetTestRunnerFor<StepExecutionTestsBindings>();

            await testRunner.GivenAsync("sample step with simple convert param: 1.23");

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK);
            bindingMock.Verify(x => x.BindingWithSimpleConvertParam(1.23));
        }

        [Fact]
        public async Task ShouldRaiseErrorIfSimpleConvertParamFails()
        {
            var (testRunner, _) = GetTestRunnerFor<StepExecutionTestsBindings>();

            await testRunner.GivenAsync("sample step with simple convert param: not-a-double");

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.TestError);
        }

        [Fact]
        public async Task ShouldCallTheOnlyThatCanConvert()
        {
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert("argument", ArgumentHelpers.IsBindingType<double>(), It.IsAny<CultureInfo>())).Returns(true);
            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync("argument", ArgumentHelpers.IsBindingType<double>(), It.IsAny<CultureInfo>())).ReturnsAsync(1.23);

            var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForArgumentConvert>();

            await testRunner.GivenAsync("sample step for argument convert: argument");


            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK, ContextManagerStub.ScenarioContext.TestError?.ToString());
            bindingMock.Verify(x => x.DoubleArg(1.23));
        }



        [Fact]
        public async Task ShouldRaiseAmbiguousIfMultipleCanConvert()
        {
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert("argument", ArgumentHelpers.IsBindingType<double>(), It.IsAny<CultureInfo>())).Returns(true);
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert("argument", ArgumentHelpers.IsBindingType<int>(), It.IsAny<CultureInfo>())).Returns(true);
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert(It.IsAny<object>(), It.IsAny<IBindingType>(), It.IsAny<CultureInfo>())).Returns(false);

            var (testRunner, _) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForArgumentConvert>();

            await testRunner.GivenAsync("sample step for argument convert: argument");

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.BindingError, ContextManagerStub.ScenarioContext.TestError?.ToString());
        }

        [Fact]
        public async Task ShouldCallTheOnlyThatCanConvertWithTable()
        {
            Table table = new Table("h1");

            // return false unless it's a Double or table->table
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert("argument", ArgumentHelpers.IsBindingType<double>(), It.IsAny<CultureInfo>())).Returns(true);

            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync(It.Is<object>(s => s.Equals(table)), ArgumentHelpers.IsBindingType<Table>(), It.IsAny<CultureInfo>())).ReturnsAsync(table);
            StepArgumentTypeConverterStub.Setup(c => c.ConvertAsync("argument", ArgumentHelpers.IsBindingType<double>(), It.IsAny<CultureInfo>())).ReturnsAsync(1.23);


            var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForArgumentConvert>();

            await testRunner.GivenAsync("sample step for argument convert with table: argument", null, table);

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK, ContextManagerStub.ScenarioContext.TestError?.ToString());
            bindingMock.Verify(x => x.DoubleArgWithTable(1.23, table));
        }

        [Fact]
        public async Task ShouldRaiseParamErrorIfNoneCanConvert()
        {
            var (testRunner, _) = GetTestRunnerFor<StepExecutionTestsBindingsForArgumentConvert>();

            // none can convert
            StepArgumentTypeConverterStub.Setup(c => c.CanConvert(It.IsAny<object>(), It.IsAny<IBindingType>(), It.IsAny<CultureInfo>())).Returns(false);

            await testRunner.GivenAsync("sample step for argument convert: argument");

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.BindingError, ContextManagerStub.ScenarioContext.TestError?.ToString());
        }
    }
}