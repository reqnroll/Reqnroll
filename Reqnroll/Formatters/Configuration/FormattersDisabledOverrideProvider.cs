using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;

namespace Reqnroll.Formatters.Configuration;

public class FormattersDisabledOverrideProvider : IFormattersConfigurationDisableOverrideProvider
{
    private readonly IEnvironmentWrapper _environmentWrapper;

    public FormattersDisabledOverrideProvider(IEnvironmentWrapper environmentWrapper)
    {
        _environmentWrapper = environmentWrapper;
    }

    public bool Disabled()
    {
        var environmentVariable = _environmentWrapper.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE);
        var disabledEnvVarValue = environmentVariable is Success<string> envVarRetrieved && bool.TryParse(envVarRetrieved.Result, out bool result) ? result : false;
        return disabledEnvVarValue;
    }
}
