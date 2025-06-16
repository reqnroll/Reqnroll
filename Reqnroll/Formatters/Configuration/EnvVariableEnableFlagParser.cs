using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Formatters.Configuration
{
    public class EnvVariableEnableFlagParser : IEnvVariableEnableFlagParser
    {
        private readonly IEnvironmentWrapper environmentWrapper;

        public EnvVariableEnableFlagParser(IEnvironmentWrapper _environmentWrapper)
        {
            environmentWrapper = _environmentWrapper;
        }

        public bool Parse()
        {
            var enabledVariable = environmentWrapper.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENABLE_ENVIRONMENT_VARIABLE);
            var enabledVariableValue = enabledVariable is Success<string> ? Convert.ToBoolean(((Success<string>)enabledVariable).Result) : true;
            return enabledVariableValue;
        }
    }

    public interface IEnvVariableEnableFlagParser
    {
        bool Parse();
    }
}
