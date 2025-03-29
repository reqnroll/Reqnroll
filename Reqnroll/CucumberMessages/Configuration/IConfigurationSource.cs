using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.Configuration
{
    internal interface IConfigurationSource
    {
        IDictionary<string, string> GetConfiguration();
    }
}
