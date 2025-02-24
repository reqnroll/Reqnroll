using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
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
        //TODO NSub fix - strange test pattern
        //[Fact]
        //public async Task ShouldCallTheUserConverterToConvertTableWithTable()
        //{
        //    var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForTableArgumentConvert>();

        //    Table table = new Table("h1");
        //    var user = new User();

        //    // return false unless its a User
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetCanConvertMethodFilter(table, typeof(User))).Returns(true);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter(table, typeof(User))).Returns(user);

        //    //bindingInstance.Expect(b => b.SingleTable(user));
        //    //MockRepository.ReplayAll();

        //    await testRunner.GivenAsync("sample step for argument convert with table", null, table);

        //    GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK);
        //    bindingMock.Received().SingleTable(user);
        //}

        //[Fact]
        //public async Task ShouldCallTheUserConverterToConvertTableWithTableAndMultilineArg()
        //{
        //    var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForTableArgumentConvert>();

        //    Table table = new Table("h1");
        //    var user = new User();
        //    var multiLineArg = "multi-line arg";
        //    // return false unless its a User
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetCanConvertMethodFilter(table, typeof(User))).Returns(true);            
        //    StepArgumentTypeConverterStub.CanConvert(Arg.Any<object>(), Arg.Any<IBindingType>(), Arg.Any<CultureInfo>()).Returns(false);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter(multiLineArg, typeof(string))).Returns(multiLineArg);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter(table, typeof(User))).Returns(user);
            

            
        //    //bindingInstance.Expect(b => b.MultilineArgumentAndTable(multiLineArg, user));
        //    //MockRepository.ReplayAll();

        //    await testRunner.GivenAsync("sample step for argument convert with multiline argument and table", multiLineArg, table);

        //    GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK);
        //    bindingMock.Received().MultilineArgumentAndTable(multiLineArg, user);
        //}

        //[Fact]
        //public async Task ShouldCallTheUserConverterToConvertTableWithTableAndMultilineArgAndParameter()
        //{
        //    var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForTableArgumentConvert>();

        //    Table table = new Table("h1");
        //    string argumentValue = "argument";
        //    var user = new User();
        //    var multiLineArg = "multi-line arg";
        //    // return false unless its a User
        //    // must also stub CanConvert & Convert for the string argument as we've introduced a parameter
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetCanConvertMethodFilter(table, typeof(User))).Returns(true);
        //    StepArgumentTypeConverterStub.CanConvert(Arg.Any<object>(), Arg.Any<IBindingType>(), Arg.Any<CultureInfo>()).Returns(false);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter(table, typeof(User))).Returns(user);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter(argumentValue, typeof(string))).Returns(argumentValue);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter(multiLineArg, typeof(string))).Returns(multiLineArg);

            
        //    //bindingInstance.Expect(b => b.ParameterMultilineArgumentAndTable(argumentValue, multiLineArg, user));
        //    //MockRepository.ReplayAll();

        //    await testRunner.GivenAsync("sample step for argument convert with parameter, multiline argument and table: argument", multiLineArg, table);

        //    GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK);
        //    bindingMock.Received().ParameterMultilineArgumentAndTable(argumentValue, multiLineArg, user);
        //}
    }
}