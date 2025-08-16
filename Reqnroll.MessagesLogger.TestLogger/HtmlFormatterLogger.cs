using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Reqnroll.FormatterTestLogger;

[FriendlyName("html-formatter")]
[ExtensionUri("logger://formatterhtmllogger")]
public class HtmlFormatterLogger : FormatterLoggerBase
{
    public HtmlFormatterLogger()
    {
        FormatterName = "html";
    }
}
