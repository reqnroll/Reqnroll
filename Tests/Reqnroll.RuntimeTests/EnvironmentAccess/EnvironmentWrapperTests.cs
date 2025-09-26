#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Reqnroll.EnvironmentAccess;
using Reqnroll.CommonModels;
using Xunit;

namespace Reqnroll.RuntimeTests.EnvironmentAccess;

[CollectionDefinition("EnvironmentWrapperTests", DisableParallelization = true)]
public class EnvironmentWrapperCollection : ICollectionFixture<EnvironmentWrapperFixture> { }

/// <summary>
/// Simple fixture (could be extended later). Ensures a single test collection disabling parallel execution.
/// </summary>
public class EnvironmentWrapperFixture { }

[Collection("EnvironmentWrapperTests")] // ensure tests here are not executed in parallel with each other
public class EnvironmentWrapperTests
{
    private readonly EnvironmentWrapper _sut = new();

    private static void Cleanup(params string[] names)
    {
        foreach (var n in names.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            Environment.SetEnvironmentVariable(n, null, EnvironmentVariableTarget.Process);
        }
    }

    [Fact]
    public void SetEnvironmentVariable_sets_process_variable()
    {
        const string name = "REQNROLL_TEST_SET";
        const string value = "VAL1";
        try
        {
            _sut.SetEnvironmentVariable(name, value);
            Environment.GetEnvironmentVariable(name).Should().Be(value);
        }
        finally
        {
            Cleanup(name);
        }
    }

    [Fact]
    public void IsEnvironmentVariableSet_returns_true_when_present()
    {
        const string name = "REQNROLL_TEST_ISSET";
        try
        {
            Environment.SetEnvironmentVariable(name, "1", EnvironmentVariableTarget.Process);
            _sut.IsEnvironmentVariableSet(name).Should().BeTrue();
        }
        finally
        {
            Cleanup(name);
        }
    }

    [Fact]
    public void IsEnvironmentVariableSet_returns_false_when_not_present()
    {
        const string name = "REQNROLL_TEST_ISSET_MISSING";
        Cleanup(name); // ensure not present
        _sut.IsEnvironmentVariableSet(name).Should().BeFalse();
    }

    [Fact]
    public void GetEnvironmentVariable_returns_success_when_present()
    {
        const string name = "REQNROLL_TEST_GET";
        const string value = "present";
        try
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
            var result = _sut.GetEnvironmentVariable(name);
            var success = result as ISuccess<string>;
            success.Should().NotBeNull();
            success!.Result.Should().Be(value);
        }
        finally
        {
            Cleanup(name);
        }
    }

    [Fact]
    public void GetEnvironmentVariable_returns_failure_when_missing()
    {
        const string name = "REQNROLL_TEST_GET_MISSING";
        Cleanup(name);
        var result = _sut.GetEnvironmentVariable(name);
        (result is IFailure<string>).Should().BeTrue();
        result.ToString().Should().Contain(name);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetEnvironmentVariables_trims_prefix_option(bool trim)
    {
        string prefix = "REQNROLL_TEST_FMT_";
        string alpha = prefix + "ALPHA";
        string betaMixedCase = "Reqnroll_Test_fmt_Beta"; // should match ignoring case
        string emptyKeyVar = prefix; // results in empty string key when trim = true
        string ignored = "X" + prefix + "IGNORED"; // does not match

        var toSet = new Dictionary<string, string>
        {
            { alpha, "1" },
            { betaMixedCase, "2" },
            { emptyKeyVar, "EMPTY" },
            { ignored, "IGNORE" }
        };

        try
        {
            foreach (var kv in toSet)
            {
                Environment.SetEnvironmentVariable(kv.Key, kv.Value, EnvironmentVariableTarget.Process);
            }

            var dict = _sut.GetEnvironmentVariables(prefix, trim);

            // Should only contain the 3 with matching prefix
            dict.Count.Should().Be(3);

            if (trim)
            {
                dict.Should().ContainKey("ALPHA").WhoseValue.Should().Be("1");
                dict.Should().ContainKey("Beta").WhoseValue.Should().Be("2"); // case preserved from original suffix
                dict.Should().ContainKey(string.Empty).WhoseValue.Should().Be("EMPTY");
                dict.Keys.Should().NotContain(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                dict.Keys.Should().Contain(k => string.Equals(k, alpha, StringComparison.OrdinalIgnoreCase));
                dict.Keys.Should().Contain(k => string.Equals(k, betaMixedCase, StringComparison.OrdinalIgnoreCase));
                dict.Keys.Should().Contain(k => string.Equals(k, emptyKeyVar, StringComparison.OrdinalIgnoreCase));
                dict.Keys.Should().NotContain(ignored);
            }
        }
        finally
        {
            Cleanup(alpha, betaMixedCase, emptyKeyVar, ignored);
        }
    }

    [Fact]
    public void GetEnvironmentVariables_throws_for_null_or_empty_prefix()
    {
        Action nullCall = () => _sut.GetEnvironmentVariables(null!);
        Action emptyCall = () => _sut.GetEnvironmentVariables(string.Empty);
        nullCall.Should().Throw<ArgumentException>();
        emptyCall.Should().Throw<ArgumentException>();
    }
}
