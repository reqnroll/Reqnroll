using Reqnroll.CommonModels;
using System.Collections.Generic;

namespace Reqnroll.EnvironmentAccess
{
    public interface IEnvironmentWrapper
    {
        IResult<string> ResolveEnvironmentVariables(string source);

        bool IsEnvironmentVariableSet(string name);

        IResult<string> GetEnvironmentVariable(string name);

        IDictionary<string,string> GetEnvironmentVariables(string prefix);

        void SetEnvironmentVariable(string name, string value);

        string GetCurrentDirectory();
    }
}
