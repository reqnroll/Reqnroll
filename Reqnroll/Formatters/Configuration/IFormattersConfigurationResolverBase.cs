using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Formatters.Configuration
{
    public interface IFormattersConfigurationResolverBase
    {
        IDictionary<string, string> Resolve();
    }

    public interface IFormattersConfigurationResolver : IFormattersConfigurationResolverBase 
    { }
    public interface IFormattersEnvironmentOverrideConfigurationResolver : IFormattersConfigurationResolverBase
    { }
}