using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;

namespace Reqnroll.Formatters.Configuration;

public class EnvVariableEnableFlagParser : IEnvVariableEnableFlagParser
{
    private readonly IEnvironmentWrapper _environmentWrapper;

    public EnvVariableEnableFlagParser(IEnvironmentWrapper environmentWrapper)
    {
        _environmentWrapper = environmentWrapper;
    }

    public bool Parse()
    {
        var enabledVariable = _environmentWrapper.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENABLE_ENVIRONMENT_VARIABLE);
        var enabledVariableValue = enabledVariable is Success<string> envVarSuccess && bool.TryParse(envVarSuccess.Result, out bool result) ? result : true;
        return enabledVariableValue;
    }
}