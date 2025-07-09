﻿using System;
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

    public bool IsEnvironmentVariableSet(string name)
        => EnvironmentVariables.ContainsKey(name);

    public IResult<string> ResolveEnvironmentVariables(string source)
        => Result<string>.Success(source);

    public void SetEnvironmentVariable(string name, string value)
        => EnvironmentVariables[name] = value;
}
