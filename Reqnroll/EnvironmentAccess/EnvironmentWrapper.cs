#nullable enable
using Reqnroll.CommonModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.EnvironmentAccess;

public class EnvironmentWrapper : IEnvironmentWrapper
{
    public bool IsEnvironmentVariableSet(string name)
    {
        return Environment.GetEnvironmentVariable(name) is not null;
    }

    public IResult<string> GetEnvironmentVariable(string name)
    {
        var variable = Environment.GetEnvironmentVariable(name);

        if (variable is not null)
        {
            return Result<string>.Success(variable);
        }

        return Result<string>.Failure($"Environment variable {name} not set");
    }

    public void SetEnvironmentVariable(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
    }

    public string GetCurrentDirectory() => Environment.CurrentDirectory;

    /// <inheritdoc/>
    public IDictionary<string, string> GetEnvironmentVariables(string prefix, bool trimPrefix = true)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentException("Argument cannot be null or empty", nameof(prefix));
        }

        return Environment.GetEnvironmentVariables()
                          .OfType<DictionaryEntry>()
                          .Select(e => (Key: e.Key?.ToString() ?? string.Empty, Value: e.Value?.ToString() ?? string.Empty))
                          .Where(e => e.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                          // If trimPrefix is true, remove the prefix from the key
                          .Select(e => trimPrefix ? (Key: e.Key.Substring(prefix.Length), e.Value) : e)
                          .ToDictionary(e => e.Key, e => e.Value);
    }
}
