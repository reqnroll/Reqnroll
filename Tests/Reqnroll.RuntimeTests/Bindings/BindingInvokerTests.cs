using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Infrastructure;
using Reqnroll.RuntimeTests.ErrorHandling;
using Reqnroll.Tracing;
using Xunit;
using Xunit.Abstractions;

namespace Reqnroll.RuntimeTests.Bindings;

public class BindingInvokerTests
{
    // ReSharper disable once NotAccessedField.Local
    private static ITestOutputHelper _testOutputHelperInstance;
    private readonly ITestOutputHelper _testOutputHelper;

    public BindingInvokerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _testOutputHelperInstance = _testOutputHelper; // to be able to access it from step def classes
    }

    private BindingInvoker CreateSut(EnvironmentWrapperStub environmentWrapperStub = null)
    {
        environmentWrapperStub ??= new EnvironmentWrapperStub();
        return new BindingInvoker(ConfigurationLoader.GetDefault(), new StubErrorProvider(), new BindingDelegateInvoker(), new EnvironmentOptions(environmentWrapperStub));
    }

    class TestableMethodBinding : MethodBinding
    {
        public TestableMethodBinding(IBindingMethod bindingMethod) : base(bindingMethod)
        {
        }
    }

    private Task<object> InvokeBindingAsync(BindingInvoker sut, IContextManager contextManager, Type stepDefType, string methodName, params object[] args)
    {
        return InvokeBindingAsync(sut, contextManager, stepDefType, methodName, new DurationHolder(), args);
    }

    private async Task<object> InvokeBindingAsync(BindingInvoker sut, IContextManager contextManager, Type stepDefType, string methodName, DurationHolder durationHolder, params object[] args)
    {
        var testTracerMock = new Mock<ITestTracer>();
        var setMethodBinding = new TestableMethodBinding(new RuntimeBindingMethod(stepDefType.GetMethod(methodName)));
        return await sut.InvokeBindingAsync(setMethodBinding, contextManager, args, testTracerMock.Object, durationHolder);
    }

    private IContextManager CreateContextManagerWith()
    {
        var contextManagerMock = new Mock<IContextManager>();
        var scenarioContainer = new ObjectContainer();
        var scenarioContext = new ScenarioContext(scenarioContainer, new ScenarioInfo("S1", null, null, null), null, new TestObjectResolver());
        contextManagerMock.SetupGet(ctx => ctx.ScenarioContext).Returns(scenarioContext);
        var contextManager = contextManagerMock.Object;
        return contextManager;
    }

    #region Generic invokation tests

    class GenericStepDefClass
    {
        public bool WasInvoked = false;
        public int CapturedIntParam = 0;
        public string CapturedStringParam = null;

        public void SyncStepDef(int intParam, string stringParam)
        {
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
        }

        public async Task AsyncStepDef(int intParam, string stringParam)
        {
            await Task.Delay(10);
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
        }

        public async Task<int> AsyncConverter(int intParam, string stringParam)
        {
            await Task.Delay(10);
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
            return 42;
        }

        public async Task<int> AsyncConverterWithoutAwait(int intParam, string stringParam)
        {
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
            return 42;
        }

        public int SyncConverter(int intParam, string stringParam)
        {
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
            return 42;
        }

        public void SyncStepDefWithException(int intParam, string stringParam)
        {
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
            throw new Exception("simulated error");
        }

        public async Task AsyncStepDefWithException(int intParam, string stringParam)
        {
            await Task.Delay(10);
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
            throw new Exception("simulated error");
        }

        public async Task AsyncStepDefWithoutAwaitWithException(int intParam, string stringParam)
        {
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
            throw new Exception("simulated error");
        }

        public async Task<int> AsyncConverterWithException(int intParam, string stringParam)
        {
            await Task.Delay(10);
            WasInvoked = true;
            CapturedIntParam = intParam;
            CapturedStringParam = stringParam;
            throw new Exception("simulated error");
        }
    }

    [Theory]
    [InlineData(nameof(GenericStepDefClass.SyncStepDef), null)]
    [InlineData(nameof(GenericStepDefClass.AsyncStepDef), null)]
    [InlineData(nameof(GenericStepDefClass.SyncConverter), 42)]
    [InlineData(nameof(GenericStepDefClass.AsyncConverter), 42)]
    [InlineData(nameof(GenericStepDefClass.AsyncConverterWithoutAwait), 42)]
    public async Task Can_invoke_different_methods(string methodName, object expectedResult)
    {
        var sut = CreateSut();
        var contextManager = CreateContextManagerWith();

        // call step definition methods
        var result = await InvokeBindingAsync(sut, contextManager, typeof(GenericStepDefClass), methodName, 24, "foo");

        // this is how to get THE instance of the step definition class
        var stepDefClass = contextManager.ScenarioContext.ScenarioContainer.Resolve<GenericStepDefClass>();
        stepDefClass.WasInvoked.Should().BeTrue();
        stepDefClass.CapturedIntParam.Should().Be(24);
        stepDefClass.CapturedStringParam.Should().Be("foo");
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(nameof(GenericStepDefClass.SyncStepDefWithException), typeof(Exception))]
    [InlineData(nameof(GenericStepDefClass.AsyncStepDefWithException), typeof(Exception))]
    [InlineData(nameof(GenericStepDefClass.AsyncStepDefWithoutAwaitWithException), typeof(Exception))]
    [InlineData(nameof(GenericStepDefClass.AsyncConverterWithException), typeof(Exception))]
    public async Task Can_invoke_different_failing_methods(string methodName, Type expectedExceptionType)
    {
        var sut = CreateSut();
        var contextManager = CreateContextManagerWith();

        // call step definition methods
        var ex = await Assert.ThrowsAsync(expectedExceptionType, () => InvokeBindingAsync(sut, contextManager, typeof(GenericStepDefClass), methodName, 24, "foo"));
        _testOutputHelper.WriteLine(ex.ToString());

        // this is how to get THE instance of the step definition class
        var stepDefClass = contextManager.ScenarioContext.ScenarioContainer.Resolve<GenericStepDefClass>();
        stepDefClass.WasInvoked.Should().BeTrue();
        stepDefClass.CapturedIntParam.Should().Be(24);
        stepDefClass.CapturedStringParam.Should().Be("foo");
    }

    #endregion

    #region ValueTask related tests

    class StepDefClassWithValueTask
    {
        public bool WasInvokedAsyncValueTaskStepDef = false;
        public bool WasInvokedAsyncValueTaskOfTStepDef = false;

        public async ValueTask AsyncValueTaskStepDef()
        {
            await Task.Delay(50); // we need to wait a bit otherwise the assertion passes even if the method is called sync
            WasInvokedAsyncValueTaskStepDef = true;
        }

        public async ValueTask<string> AsyncValueTaskOfTStepDef()
        {
            await Task.Delay(50); // we need to wait a bit otherwise the assertion passes even if the method is called sync
            WasInvokedAsyncValueTaskOfTStepDef = true;
            return "foo";
        }
    }

    [Fact]
    public async Task Async_methods_of_ValueTask_return_type_can_be_invoked()
    {
        var sut = CreateSut();
        var contextManager = CreateContextManagerWith();

        // call step definition methods
        await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassWithValueTask), nameof(StepDefClassWithValueTask.AsyncValueTaskStepDef));

        // this is how to get THE instance of the step definition class
        var stepDefClass = contextManager.ScenarioContext.ScenarioContainer.Resolve<StepDefClassWithValueTask>();
        stepDefClass.WasInvokedAsyncValueTaskStepDef.Should().BeTrue();
    }

    [Fact]
    public async Task Async_methods_of_ValueTaskOfT_return_type_can_be_invoked()
    {
        var sut = CreateSut();
        var contextManager = CreateContextManagerWith();

        await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassWithValueTask), nameof(StepDefClassWithValueTask.AsyncValueTaskOfTStepDef));

        var stepDefClass = contextManager.ScenarioContext.ScenarioContainer.Resolve<StepDefClassWithValueTask>();
        stepDefClass.WasInvokedAsyncValueTaskOfTStepDef.Should().BeTrue();
    }

    #endregion

    #region Async void related tests

    class StepDefClassWithAsyncVoid
    {
        public static bool WasInvokedAsyncVoidStepDef = false;

        public async void AsyncVoidStepDef()
        {
            await Task.Delay(50); // we need to wait a bit otherwise the assertion passes even if the method is called sync
            WasInvokedAsyncVoidStepDef = true;
        }
    }

    [Fact]
    public async Task Async_void_binding_methods_are_not_supported()
    {
        var sut = CreateSut();
        var contextManager = CreateContextManagerWith();

        await FluentActions.Awaiting(() => InvokeBindingAsync(sut, contextManager, typeof(StepDefClassWithAsyncVoid), nameof(StepDefClassWithAsyncVoid.AsyncVoidStepDef)))
                           .Should()
                           .ThrowAsync<ReqnrollException>()
                           .WithMessage("*async void*");

        StepDefClassWithAsyncVoid.WasInvokedAsyncVoidStepDef.Should().BeFalse();
    }

    #endregion

    #region ExecutionContext / AsyncLocal<T> related tests

    public enum AsyncLocalType
    {
        Uninitialized,
        CtorInitialized,
        Boxed
    }

    class StepDefClassWithAsyncLocal
    {
        private readonly AsyncLocal<string> _uninitializedAsyncLocal = new();
        private readonly AsyncLocal<string> _ctorInitializedAsyncLocal = new() { Value = "ctor-value" };
        private readonly AsyncLocal<StrongBox<string>> _boxedAsyncLocal = new() { Value = new StrongBox<string>("ctor-value") };

        public string LoadedValue { get; set; }

        // ReSharper disable once UnusedMember.Local
        public void SetAsyncLocal_Sync(AsyncLocalType asyncLocalType, string content)
        {
            switch (asyncLocalType)
            {
                case AsyncLocalType.Uninitialized:
                    _uninitializedAsyncLocal.Value = content;
                    break;
                case AsyncLocalType.CtorInitialized:
                    _ctorInitializedAsyncLocal.Value = content;
                    break;
                case AsyncLocalType.Boxed:
                    _boxedAsyncLocal.Value!.Value = content;
                    break;
            }
        }

        // ReSharper disable once UnusedMember.Local
        public async Task SetAsyncLocal_Async(AsyncLocalType asyncLocalType, string content)
        {
            switch (asyncLocalType)
            {
                case AsyncLocalType.Uninitialized:
                    _uninitializedAsyncLocal.Value = content;
                    break;
                case AsyncLocalType.CtorInitialized:
                    _ctorInitializedAsyncLocal.Value = content;
                    break;
                case AsyncLocalType.Boxed:
                    _boxedAsyncLocal.Value!.Value = content;
                    break;
            }
            await Task.Delay(1);
        }

        public async Task GetAsyncLocal_Async(AsyncLocalType asyncLocalType, string expectedResult)
        {
            switch (asyncLocalType)
            {
                case AsyncLocalType.Uninitialized:
                    LoadedValue = _uninitializedAsyncLocal.Value;
                    break;
                case AsyncLocalType.CtorInitialized:
                    LoadedValue = _ctorInitializedAsyncLocal.Value;
                    break;
                case AsyncLocalType.Boxed:
                    LoadedValue = _boxedAsyncLocal.Value?.Value;
                    break;
            }
            Assert.Equal(expectedResult, LoadedValue);
            await Task.Delay(1);
        }
    }

    [Theory]
    [InlineData("Value is initialized in ctor", AsyncLocalType.CtorInitialized, null, "ctor-value")]
    [InlineData("Value is initialized in ctor and also set in sync step def", AsyncLocalType.CtorInitialized, "Sync", "42")]
    [InlineData("Value is initialized in ctor and also set in async step def", AsyncLocalType.CtorInitialized, "Async", "ctor-value")] // the change in async step def is discarded
    [InlineData("Value is set in sync step def", AsyncLocalType.Uninitialized, "Sync", "42")]
    [InlineData("Value is set in async step def", AsyncLocalType.Uninitialized, "Async", null)] // the change in async step def is discarded
    [InlineData("Boxed value is initialized in ctor", AsyncLocalType.Boxed, null, "ctor-value")]
    [InlineData("Boxed value is initialized in ctor and also set in sync step def", AsyncLocalType.Boxed, "Sync", "42")]
    [InlineData("Boxed value is initialized in ctor and also set in async step def", AsyncLocalType.Boxed, "Async", "42")] // the change in async step def is preserved because of boxing
    public async Task ExecutionContext_is_flowing_down_correctly_to_step_definitions(string description, AsyncLocalType asyncLocalType, string setAs, string expectedResult)
    {
        _testOutputHelper.WriteLine(description);
        var sut = CreateSut();
        var contextManager = CreateContextManagerWith();

        if (setAs != null)
            await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassWithAsyncLocal), "SetAsyncLocal_" + setAs, asyncLocalType, "42");
        await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassWithAsyncLocal), nameof(StepDefClassWithAsyncLocal.GetAsyncLocal_Async), asyncLocalType, expectedResult);

        var stepDefClass = contextManager.ScenarioContext.ScenarioContainer.Resolve<StepDefClassWithAsyncLocal>();
        stepDefClass.LoadedValue.Should().Be(expectedResult, $"Error was not propagated for {description}");
    }

    [Fact]
    public async Task ExecutionContext_can_be_changed_multiple_times()
    {
        var sut = CreateSut();
        var contextManager = CreateContextManagerWith();

        await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassWithAsyncLocal), "SetAsyncLocal_Sync", AsyncLocalType.Uninitialized, "14");
        await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassWithAsyncLocal), "SetAsyncLocal_Sync", AsyncLocalType.Uninitialized, "42");
        await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassWithAsyncLocal), nameof(StepDefClassWithAsyncLocal.GetAsyncLocal_Async), AsyncLocalType.Uninitialized, "42");
        var stepDefClass = contextManager.ScenarioContext.ScenarioContainer.Resolve<StepDefClassWithAsyncLocal>();
        stepDefClass.LoadedValue.Should().Be("42", $"Error was not propagated");
    }

    #endregion   

    #region Exception Handling related tests - regression tests for SF2649
    public enum ExceptionKind
    {
        Normal,
        Aggregate
    }

    public enum InnerExceptionContentKind
    {
        WithInnerException,
        WithoutInnerException
    }

    public enum StepDefInvocationStyle
    {
        Sync,
        Async
    }

    class StepDefClassThatThrowsExceptions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task AsyncThrow(ExceptionKind kindOfExceptionToThrow, InnerExceptionContentKind innerExceptionKind)
        {
            await Task.Run(() => ConstructAndThrowSync(kindOfExceptionToThrow, innerExceptionKind));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SyncThrow(ExceptionKind kindOfExceptionToThrow, InnerExceptionContentKind innerExceptionKind)
        {
            ConstructAndThrowSync(kindOfExceptionToThrow, innerExceptionKind);
        }

        private void ConstructAndThrowSync(ExceptionKind typeOfExceptionToThrow, InnerExceptionContentKind innerExceptionContentKind)
        {
            switch (typeOfExceptionToThrow, innerExceptionContentKind)
            {
                case (ExceptionKind.Normal, InnerExceptionContentKind.WithoutInnerException):
                    throw new Exception("Normal Exception message (No InnerException expected).");

                case (ExceptionKind.Aggregate, InnerExceptionContentKind.WithoutInnerException):
                    throw new AggregateException("AggregateEx (without Inners)");

                case (ExceptionKind.Normal, InnerExceptionContentKind.WithInnerException):
                    try
                    {
                        throw new Exception("This is the message from the Inner Exception");
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Normal Exception (with InnerException)", e);
                    }

                case (ExceptionKind.Aggregate, InnerExceptionContentKind.WithInnerException):
                    {
                        var tasks = new List<Task>
                        {
                            Task.Run(async () => throw new Exception("This is the first Exception embedded in the AggregateException")),
                            Task.Run(async () => throw new Exception("This is the second Exception embedded in the AggregateException"))
                        };
                        var continuation = Task.WhenAll(tasks);

                        // This will throw an AggregateException with two Inner Exceptions
                        continuation.Wait();
                        return;
                    }
            }

        }
    }

    [Theory]
    [InlineData(StepDefInvocationStyle.Sync, InnerExceptionContentKind.WithoutInnerException)]
    [InlineData(StepDefInvocationStyle.Sync, InnerExceptionContentKind.WithInnerException)]
    [InlineData(StepDefInvocationStyle.Async, InnerExceptionContentKind.WithoutInnerException)]
    [InlineData(StepDefInvocationStyle.Async, InnerExceptionContentKind.WithInnerException)]
    public async Task InvokeBindingAsync_WhenStepDefThrowsExceptions_ProperlyPreservesExceptionContext(StepDefInvocationStyle style, InnerExceptionContentKind inner)
    {
        _testOutputHelper.WriteLine($"starting Exception Handling test: {style}, {inner}");
        var sut = CreateSut();
        var contextManager = CreateContextManagerWith();

        string methodToInvoke;
        ExceptionKind kindOfExceptionToThrow;
        Exception thrown;

        // call step definition methods
        if (style == StepDefInvocationStyle.Sync)
        {
            methodToInvoke = nameof(StepDefClassThatThrowsExceptions.SyncThrow);
            kindOfExceptionToThrow = ExceptionKind.Normal;
            thrown = await Assert.ThrowsAsync<Exception>(async () => await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassThatThrowsExceptions), methodToInvoke, kindOfExceptionToThrow, inner));
        }
        else // if (style == StepDefInvocationStyle.Async)
        {
            methodToInvoke = nameof(StepDefClassThatThrowsExceptions.AsyncThrow);
            kindOfExceptionToThrow = ExceptionKind.Aggregate;
            thrown = await Assert.ThrowsAsync<AggregateException>(async () => await InvokeBindingAsync(sut, contextManager, typeof(StepDefClassThatThrowsExceptions), methodToInvoke, kindOfExceptionToThrow, inner));
        }

        _testOutputHelper.WriteLine($"Exception detail: {thrown}");

        // Assert that the InnerException detail is preserved
        if (inner == InnerExceptionContentKind.WithInnerException)
        {
            if (thrown is AggregateException) (thrown as AggregateException).InnerExceptions.Count.Should().BeGreaterThan(1);
            else thrown.InnerException.Should().NotBeNull();
        }

        // Assert that the stack trace properly shows that the exception came from the throwing method (and not hidden by the Reqnroll infrastructure)
        thrown.StackTrace.Should().Contain(methodToInvoke);
    }
    #endregion

    #region Dry run tests

    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    public async Task ShouldSkipExecutingStepsWhenDryRunEnabled(string value)
    {
        var envStub = new EnvironmentWrapperStub();
        envStub.EnvironmentVariables.Add(EnvironmentOptions.REQNROLL_DRY_RUN_ENVIRONMENT_VARIABLE, value);
        var sut = CreateSut(environmentWrapperStub: envStub);
        var contextManager = CreateContextManagerWith();
        var durationHolder = new DurationHolder();

        // invoke, but with dry run execution we expect no exception thrown
        var result = await InvokeBindingAsync(sut, contextManager, typeof(GenericStepDefClass), nameof(GenericStepDefClass.AsyncStepDefWithException), durationHolder, 1, "foo");

        // verify dry run execution
        result.Should().BeNull();
        durationHolder.Duration.Should().Be(TimeSpan.Zero);
        var stepDefClass = contextManager.ScenarioContext.ScenarioContainer.Resolve<GenericStepDefClass>();
        stepDefClass.WasInvoked.Should().BeFalse();
        stepDefClass.CapturedIntParam.Should().Be(0);
        stepDefClass.CapturedStringParam.Should().BeNull();
    }

    [Theory]
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData((string)null)]
    public async Task ShouldExecuteStepsWhenDryRunDisabled(string value)
    {
        var envStub = new EnvironmentWrapperStub();
        envStub.EnvironmentVariables.Add(EnvironmentOptions.REQNROLL_DRY_RUN_ENVIRONMENT_VARIABLE, value);
        var sut = CreateSut(environmentWrapperStub: envStub);
        var contextManager = CreateContextManagerWith();
        var durationHolder = new DurationHolder();

        // invoke, but expect dry run execution
        var result = await InvokeBindingAsync(sut, contextManager, typeof(GenericStepDefClass), nameof(GenericStepDefClass.AsyncConverter), durationHolder, 1, "foo");

        // verify dry run execution
        result.Should().Be(42);
        durationHolder.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        var stepDefClass = contextManager.ScenarioContext.ScenarioContainer.Resolve<GenericStepDefClass>();
        stepDefClass.WasInvoked.Should().BeTrue();
        stepDefClass.CapturedIntParam.Should().Be(1);
        stepDefClass.CapturedStringParam.Should().Be("foo");
    }

    #endregion
}