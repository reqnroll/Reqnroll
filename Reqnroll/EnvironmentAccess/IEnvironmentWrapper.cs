#nullable enable
using Reqnroll.CommonModels;
using System.Collections.Generic;

namespace Reqnroll.EnvironmentAccess;

public interface IEnvironmentWrapper
{
    bool IsEnvironmentVariableSet(string name);

    IResult<string> GetEnvironmentVariable(string name);

    /// <summary>
    /// Retrieves all environment variables that start with the specified prefix,
    /// optionally removing the prefix from the keys in the returned dictionary.
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="trimPrefix">If true, the prefix will be removed from the keys in the returned dictionary.</param>
    IDictionary<string,string> GetEnvironmentVariables(string prefix, bool trimPrefix = true);

    string GetCurrentDirectory();
}
