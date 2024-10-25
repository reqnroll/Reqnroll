using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Extensions.Configuration;
using Reqnroll.Configuration.JsonConfig;

namespace Reqnroll.Configuration
{
    internal class MSE_RuntimeConfigurationLoader
    {
        public ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration, string configPath)
        {
            var configurationManager = new ConfigurationManager();
            configurationManager.AddJsonFile(configPath, optional: true, reloadOnChange: false);
            configurationManager.AddEnvironmentVariables(prefix: "REQNROLL__");
            //configurationManager.Build();
            var config = configurationManager.Get<JsonConfig.JsonConfig>();
            return new JsonConfigurationLoader().LoadJson(reqnrollConfiguration, config);
        }
    }
}
