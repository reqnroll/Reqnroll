using System;
using System.Collections.Generic;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;

namespace Reqnroll.RuntimeTests;

public class EnvironmentWrapperStub : IEnvironmentWrapper
{
    /// <summary>
    /// Allows changing the current directory for a test run.
    /// </summary>
    public string CurrentDirectory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// Allows configuring the environment variables for a test run.
    /// </summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string GetCurrentDirectory()
        => CurrentDirectory;

    public IResult<string> GetEnvironmentVariable(string name)
        => EnvironmentVariables.TryGetValue(name, out var value)
            ? Result<string>.Success(value)
            : Result<string>.Failure($"Environment variable '{name}' not set in stub");

    public IDictionary<string, string> GetEnvironmentVariables(string prefix, bool trimPrefix = true) => throw new NotSupportedException();

    public bool IsEnvironmentVariableSet(string name)
        => EnvironmentVariables.ContainsKey(name);

    public void SetEnvironmentVariable(string name, string value)
        => EnvironmentVariables[name] = value;
}
