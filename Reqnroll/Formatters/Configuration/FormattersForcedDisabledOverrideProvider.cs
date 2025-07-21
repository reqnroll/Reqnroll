namespace Reqnroll.Formatters.Configuration;

public class FormattersForcedDisabledOverrideProvider : IFormattersConfigurationDisableOverrideProvider
{
    public bool Disabled()
    {
        return true;
    }
}