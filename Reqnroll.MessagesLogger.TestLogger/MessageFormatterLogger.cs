using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Reqnroll.FormatterTestLogger;

[FriendlyName("message-formatter")]
[ExtensionUri("logger://formattermessagelogger")]
public class MessageFormatterLogger : FormatterLoggerBase
{
    public MessageFormatterLogger()
    {
        FormatterName = "message";
    }
}
