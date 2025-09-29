#nullable enable
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Xunit;

namespace Reqnroll.RuntimeTests.EnvironmentAccess;

public class EnvironmentOptionsTests
{
    private static EnvironmentOptions CreateSut(Mock<IEnvironmentWrapper> envMock) =>
        new EnvironmentOptions(envMock.Object);

    private static IResult<string> Success(string value) => Result<string>.Success(value);
    private static IResult<string> Failure(string message = "fail") => Result<string>.Failure(message);

    [Fact]
    public void IsDryRun_should_be_true_when_env_var_parses_true()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_DRY_RUN_ENVIRONMENT_VARIABLE))
           .Returns(Success("true"));

        CreateSut(env).IsDryRun.Should().BeTrue();
    }

    [Theory]
    [InlineData("false")]
    [InlineData("FALSE")]
    [InlineData("False")]
    public void IsDryRun_should_be_false_when_env_var_parses_false(string value)
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_DRY_RUN_ENVIRONMENT_VARIABLE))
           .Returns(Success(value));

        CreateSut(env).IsDryRun.Should().BeFalse();
    }

    [Fact]
    public void IsDryRun_should_be_false_when_env_var_invalid()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_DRY_RUN_ENVIRONMENT_VARIABLE))
           .Returns(Success("notabool"));

        CreateSut(env).IsDryRun.Should().BeFalse();
    }

    [Fact]
    public void IsDryRun_should_be_false_when_env_var_missing()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_DRY_RUN_ENVIRONMENT_VARIABLE))
           .Returns(Failure());

        CreateSut(env).IsDryRun.Should().BeFalse();
    }

    [Fact]
    public void BindingsOutputFilepath_should_return_value_when_success()
    {
        var path = "c:\\temp\\bindings.json";
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_BINDING_OUTPUT_ENVIRONMENT_VARIABLE))
           .Returns(Success(path));

        CreateSut(env).BindingsOutputFilepath.Should().Be(path);
    }

    [Fact]
    public void BindingsOutputFilepath_should_be_null_when_missing()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_BINDING_OUTPUT_ENVIRONMENT_VARIABLE))
           .Returns(Failure());

        CreateSut(env).BindingsOutputFilepath.Should().BeNull();
    }

    [Fact]
    public void IsRunningInContainer_should_reflect_wrapper_flag_true()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.IsEnvironmentVariableSet(EnvironmentOptions.DOTNET_RUNNING_IN_CONTAINER_ENVIRONMENT_VARIABLE))
           .Returns(true);

        CreateSut(env).IsRunningInContainer.Should().BeTrue();
    }

    [Fact]
    public void IsRunningInContainer_should_reflect_wrapper_flag_false()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.IsEnvironmentVariableSet(EnvironmentOptions.DOTNET_RUNNING_IN_CONTAINER_ENVIRONMENT_VARIABLE))
           .Returns(false);

        CreateSut(env).IsRunningInContainer.Should().BeFalse();
    }

    [Fact]
    public void FormattersDisabled_should_be_true_when_env_var_true()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE))
           .Returns(Success("TRUE"));

        CreateSut(env).FormattersDisabled.Should().BeTrue();
    }

    [Theory]
    [InlineData("false")]
    [InlineData("FALSE")]
    [InlineData("False")]
    public void FormattersDisabled_should_be_false_when_env_var_false(string value)
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE))
           .Returns(Success(value));

        CreateSut(env).FormattersDisabled.Should().BeFalse();
    }

    [Fact]
    public void FormattersDisabled_should_be_false_when_env_var_invalid()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE))
           .Returns(Success("notabool"));

        CreateSut(env).FormattersDisabled.Should().BeFalse();
    }

    [Fact]
    public void FormattersDisabled_should_be_false_when_missing()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE))
           .Returns(Failure());

        CreateSut(env).FormattersDisabled.Should().BeFalse();
    }

    [Fact]
    public void FormattersJson_should_return_value_when_set()
    {
        var json = "{ \"formatters\": {} }";
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE))
           .Returns(Success(json));

        CreateSut(env).FormattersJson.Should().Be(json);
    }

    [Fact]
    public void FormattersJson_should_be_null_when_missing()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE))
           .Returns(Failure());

        CreateSut(env).FormattersJson.Should().BeNull();
    }

    [Fact]
    public void FormatterSettings_should_return_dictionary_from_wrapper_and_use_expected_prefix()
    {
        var expected = new Dictionary<string, string>
        {
            { "HTML__outputFilePath", "out.html" },
            { "MESSAGE__outputFilePath", "out.ndjson" }
        };

        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariables(EnvironmentOptions.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX, true))
           .Returns(expected);

        var result = CreateSut(env).FormatterSettings;

        result.Should().BeEquivalentTo(expected);
        env.Verify(e => e.GetEnvironmentVariables(EnvironmentOptions.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX, true), Times.Once);
    }

    [Fact]
    public void FormatterSettings_should_return_empty_dictionary_when_wrapper_returns_empty()
    {
        var env = new Mock<IEnvironmentWrapper>();
        env.Setup(e => e.GetEnvironmentVariables(EnvironmentOptions.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX, true))
           .Returns(new Dictionary<string, string>());

        CreateSut(env).FormatterSettings.Should().BeEmpty();
    }
}
