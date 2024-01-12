using System;

namespace Reqnroll.ExternalData.ReqnrollPlugin
{
    public class ExternalDataPluginException : Exception
    {
        public ExternalDataPluginException(string message) : base(message)
        {
        }

        public ExternalDataPluginException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
