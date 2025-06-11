using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.Configuration
{
    public interface ICucumberMessagesConfigurationResolverBase
    {
        IDictionary<string, string> Resolve();
    }

    public interface ICucumberMessagesConfigurationResolver : ICucumberMessagesConfigurationResolverBase 
    { }
    public interface ICucumberMessagesEnvironmentOverrideConfigurationResolver : ICucumberMessagesConfigurationResolverBase
    { }
}