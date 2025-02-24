using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using NSubstitute;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.RuntimeTests
{
    //TODO NSub fix
    //internal static class LegacyStepArgumentTypeConverterExtensions
    //{
    //    public static async Task<object> ConvertAsync(this IStepArgumentTypeConverter converter, object value, Type typeToConvertTo, CultureInfo cultureInfo)
    //    {
    //        return await converter.ConvertAsync(value, new RuntimeBindingType(typeToConvertTo), cultureInfo);
    //    }

    //    public static Expression<Func<IStepArgumentTypeConverter, bool>> GetCanConvertMethodFilter(object argument, Type type)
    //    {
    //        return c => c.CanConvert(argument, Arg.Is<IBindingType>(bt => bt.TypeEquals(type)), Arg.Any<CultureInfo>());
    //    }

    //    public static Expression<Func<IStepArgumentTypeConverter, Task<object>>> GetConvertAsyncMethodFilter(object argument, Type type)
    //    {
    //        return c => c.ConvertAsync(Arg.Is<object>(s => s.Equals(argument)), Arg.Is<IBindingType>(bt => bt.TypeEquals(type)), Arg.Any<CultureInfo>());
    //            //Arg<string>.Is.Equal(argument),
    //            //Arg<IBindingType>.Matches(bt => bt.TypeEquals(type)), 
    //            //Arg<CultureInfo>.Is.Anything);
    //    }     
    //}

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

            //bindingInstance.Expect(b => b.BindingWithSimpleConvertParam(1.23));

            //MockRepository.ReplayAll();

            await testRunner.GivenAsync("sample step with simple convert param: 1.23");

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK);
            bindingMock.Received().BindingWithSimpleConvertParam(1.23);
        }

        [Fact]
        public async Task ShouldRaiseErrorIfSimpleConvertParamFails()
        {
            var (testRunner, bindingMock) = GetTestRunnerFor<StepExecutionTestsBindings>();

            //MockRepository.ReplayAll();

            await testRunner.GivenAsync("sample step with simple convert param: not-a-double");

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.TestError);
        }

        //TODO NSub fix
        //[Fact]
        //public async Task ShouldCallTheOnlyThatCanConvert()
        //{
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetCanConvertMethodFilter("argument", typeof(double))).Returns(true);
        //    //StepArgumentTypeConverterStub.CanConvert(Arg.Any<object>(), Arg.Any<IBindingType>(), Arg.Any<CultureInfo>()).Returns(false);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter("argument", typeof(double))).Returns(1.23);

        //    var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForArgumentConvert>();

            


        //    //bindingInstance.Expect(b => b.DoubleArg(1.23));

        //    //MockRepository.ReplayAll();

        //    await testRunner.GivenAsync("sample step for argument convert: argument");


        //    GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK, ContextManagerStub.ScenarioContext.TestError?.ToString());
        //    bindingMock.Received().DoubleArg(1.23);
        //    //StepArgumentTypeConverterStub.Received().Convert("argument", Arg.Is<IBindingType>(bt => bt.TypeEquals(typeof(double))), Arg.Any<CultureInfo>())).; LegacyStepArgumentTypeConverterExtensions.GetConvertMethodFilter("argument", typeof(double)));
        //}

       
        //TODO NSub fix
        //[Fact]
        //public async Task ShouldRaiseAmbiguousIfMultipleCanConvert()
        //{
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetCanConvertMethodFilter("argument", typeof(double))).Returns(true);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetCanConvertMethodFilter("argument", typeof(int))).Returns(true);
        //    StepArgumentTypeConverterStub.CanConvert(Arg.Any<object>(), Arg.Any<IBindingType>(), Arg.Any<CultureInfo>()).Returns(false);

        //    var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForArgumentConvert>();

        //    // return false unless its a Double or an Int
            
        //    //MockRepository.ReplayAll();

        //    await testRunner.GivenAsync("sample step for argument convert: argument");


        //    GetLastTestStatus().Should().Be(ScenarioExecutionStatus.BindingError, ContextManagerStub.ScenarioContext.TestError?.ToString());
        //}

        //TODO NSub fix
        //[Fact]
        //public async Task ShouldCallTheOnlyThatCanConvertWithTable()
        //{
        //    Table table = new Table("h1");

        //    // return false unless its a Double or table->table
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetCanConvertMethodFilter("argument", typeof(double))).Returns(true);
        //    //StepArgumentTypeConverterStub.CanConvert(Arg.Any<object>(), Arg.Any<IBindingType>(), Arg.Any<CultureInfo>()).Returns(false);

        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter(table, typeof(Table))).Returns(table);
        //    StepArgumentTypeConverterStub.Setup(LegacyStepArgumentTypeConverterExtensions.GetConvertAsyncMethodFilter("argument", typeof(double))).Returns(1.23);


        //    var (testRunner, bindingMock) = GetTestRunnerWithConverterStub<StepExecutionTestsBindingsForArgumentConvert>();

            
        //    //bindingInstance.Expect(b => b.DoubleArgWithTable(1.23, table));

        //    //MockRepository.ReplayAll();

        //    await testRunner.GivenAsync("sample step for argument convert with table: argument", null, table);


        //    GetLastTestStatus().Should().Be(ScenarioExecutionStatus.OK, ContextManagerStub.ScenarioContext.TestError?.ToString());
        //    bindingMock.Received().DoubleArgWithTable(1.23, table);
        //}

        [Fact]
        public async Task ShouldRaiseParamErrorIfNoneCanConvert()
        {
            var (testRunner, bindingMock) = GetTestRunnerFor<StepExecutionTestsBindingsForArgumentConvert>();

            // none can convert
            StepArgumentTypeConverterStub.CanConvert(Arg.Any<object>(), Arg.Any<IBindingType>(), Arg.Any<CultureInfo>()).Returns(false);

           // MockRepository.ReplayAll();

            await testRunner.GivenAsync("sample step for argument convert: argument");

            GetLastTestStatus().Should().Be(ScenarioExecutionStatus.BindingError, ContextManagerStub.ScenarioContext.TestError?.ToString());
        }
    }
}