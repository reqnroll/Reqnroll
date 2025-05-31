using System;
using System.Collections.Generic;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;

namespace Reqnroll.RuntimeTests;

public class EnvironmentWrapperStub : IEnvironmentWrapper
{
    public string CurrentDirectory { get; set; } = @"C:\foo\bar";
    public IDictionary<string, string> Environment { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string GetCurrentDirectory() 
        => CurrentDirectory;

    public IResult<string> GetEnvironmentVariable(string name)
        => Environment.TryGetValue(name, out var value) 
            ? Result<string>.Success(value) 
            : Result<string>.Failure($"Environment variable '{name}' not set in stub");

    public bool IsEnvironmentVariableSet(string name)
        => Environment.ContainsKey(name);

    public IResult<string> ResolveEnvironmentVariables(string source)
        => Result<string>.Success(source);

    public void SetEnvironmentVariable(string name, string value)
        => Environment[name] = value;
}
