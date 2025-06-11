using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Formatters.Configuration
{
    public class EnvVariableEnableFlagParser(IEnvironmentWrapper _environmentWrapper) : IEnvVariableEnableFlagParser
    {
        public bool Parse()
        {
            var enabledVariable = _environmentWrapper.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENABLE_ENVIRONMENT_VARIABLE);
            var enabledVariableValue = enabledVariable is Success<string> ? Convert.ToBoolean(((Success<string>)enabledVariable).Result) : true;
            return enabledVariableValue;
        }
    }

    public interface IEnvVariableEnableFlagParser
    {
        bool Parse();
    }
}
