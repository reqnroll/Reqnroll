using Reqnroll.EnvironmentAccess;

namespace Reqnroll.Formatters.Configuration;

public class FormattersDisabledOverrideProvider(IEnvironmentOptions environmentOptions) : IFormattersConfigurationDisableOverrideProvider
{
    public bool Disabled()
    {
        return environmentOptions.FormattersDisabled;
    }
}
