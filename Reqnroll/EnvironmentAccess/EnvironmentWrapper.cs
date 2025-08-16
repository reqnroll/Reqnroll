using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.CommonModels;

namespace Reqnroll.EnvironmentAccess
{
    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public IResult<string> ResolveEnvironmentVariables(string source)
        {
            if (source is null)
            {
                return Result<string>.Failure(new ArgumentNullException(nameof(source)));
            }

            return Result<string>.Success(Environment.ExpandEnvironmentVariables(source));
        }

        public bool IsEnvironmentVariableSet(string name)
        {
            return Environment.GetEnvironmentVariables().Contains(name);
        }

        public IResult<string> GetEnvironmentVariable(string name)
        {
            if (IsEnvironmentVariableSet(name))
            {
                return Result<string>.Success(Environment.GetEnvironmentVariable(name));
            }

            return Result<string>.Failure($"Environment variable {name} not set");
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        }

        public string GetCurrentDirectory() => Environment.CurrentDirectory;

        public IReadOnlyCollection<string> GetEnvironmentVariableNames(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new ArgumentException("Argument cannot be null or empty", nameof(prefix));

            return Environment.GetEnvironmentVariables()
                              .Keys
                              .OfType<string>()
                              .Where(k => k.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                              .ToArray();
        }
    }
}
