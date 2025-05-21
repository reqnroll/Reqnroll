using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reqnroll.BoDi;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Infrastructure;
using Reqnroll.InternalPlugins;
using Xunit;

namespace Reqnroll.RuntimeTests.InternalPlugins;

[Binding]
public class TrackedStepExecutionBindings
{
    public int StepExecutionCount { get; private set; } = 0;

    [Given("I have some stuff setup")]
    public virtual void Given()
    {
        StepExecutionCount++;
    }
}

public class DryRunStepExecutionTests : StepExecutionTestsBase
{
    // This is a mock of the IEnvironmentWrapper interface, which is used to access environment variables.
    private readonly Mock<IEnvironmentWrapper> _environmentWrapperMock = new Mock<IEnvironmentWrapper>();

    // This class is used to provide a custom runtime configuration for the test runner.
    // Specifically, it overrides the default dependency provider to use the mocked environment wrapper.
    class DryRunRuntimeConfigurationProvider : DefaultDependencyProvider
    {
        private readonly IEnvironmentWrapper _environmentWrapperMock;

        public DryRunRuntimeConfigurationProvider(IEnvironmentWrapper environmentWrapperMock)
        {
            _environmentWrapperMock = environmentWrapperMock;
        }

        public override void RegisterGlobalContainerDefaults(ObjectContainer container)
        {
            base.RegisterGlobalContainerDefaults(container);
            container.RegisterInstanceAs<IEnvironmentWrapper>(_environmentWrapperMock);
        }
    }

    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    public async Task ShouldSkipExecutingStepsWhenDryRunEnabled(string value)
    {
        _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(DryRunBindingInvokerPlugin.DryRunEnvVarName)).Returns(Result<string>.Success(value));

        var (testRunner, bindingMock) = GetTestRunnerFor<TrackedStepExecutionBindings>(new DryRunRuntimeConfigurationProvider(_environmentWrapperMock.Object));
        bindingMock.CallBase = true;
        bindingMock.SetupAllProperties();

        await testRunner.GivenAsync("I have some stuff setup");

        bindingMock.Verify(x => x.Given(), Times.Never, "Step should not be executed in dry run mode.");
        bindingMock.Object.StepExecutionCount.Should().Be(0, "Step should not be executed in dry run mode.");
    }

    [Theory]
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData((string)null)]
    public async Task ShouldExecuteStepsWhenDryRunDisabled(string value)
    {
        IResult<string> result = value == null ? Result<string>.Failure("Environment variable not set") : Result<string>.Success(value);
        _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable(DryRunBindingInvokerPlugin.DryRunEnvVarName)).Returns(result);

        var (testRunner, bindingMock) = GetTestRunnerFor<TrackedStepExecutionBindings>(new DryRunRuntimeConfigurationProvider(_environmentWrapperMock.Object));
        bindingMock.CallBase = true;
        bindingMock.SetupAllProperties();

        await testRunner.GivenAsync("I have some stuff setup");

        bindingMock.Verify(x => x.Given(), Times.Once, "Step should be executed when dry run disabled.");
        bindingMock.Object.StepExecutionCount.Should().Be(1, "Step should be executed when dry run disabled.");
    }
}
