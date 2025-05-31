using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.Configuration
{
    public interface ICucumberMessagesConfigurationResolver
    {
        IDictionary<string, string> Resolve();
    }
}