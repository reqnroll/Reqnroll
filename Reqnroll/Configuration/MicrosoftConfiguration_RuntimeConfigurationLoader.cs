using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Extensions.Configuration;
using Reqnroll.Configuration.JsonConfig;
using Reqnroll.Tracing;
using SpecFlow.Internal.Json;

namespace Reqnroll.Configuration
{
    public class MicrosoftConfiguration_RuntimeConfigurationLoader : IMS_ConfigurationLoader
     {
        private readonly string _jsonConfigurationFilePath;
        private readonly IConfigurationManager _configurationManager;

        public MicrosoftConfiguration_RuntimeConfigurationLoader(IReqnrollJsonLocator reqnrollJsonLocator, IConfigurationManager configurationManager)
        {
            _jsonConfigurationFilePath = reqnrollJsonLocator.GetReqnrollJsonFilePath();
            if (_jsonConfigurationFilePath != null)
            {
                configurationManager.AddJsonFile(_jsonConfigurationFilePath, optional: true, reloadOnChange: false);
                configurationManager.AddEnvironmentVariables(prefix: "REQNROLL__");
                _configurationManager = configurationManager;
                //configurationManager.Build();
                // Build not necessary as IConfigurationManager automatically rebuilds when sources are changed
            }

        }


        public ReqnrollConfiguration Load(ReqnrollConfiguration reqnrollConfiguration)
        {
            var config = _configurationManager.Get<JsonConfig.JsonConfig>();
            var configuration = JsonConfigurationLoader.ApplyJsonConfig(reqnrollConfiguration, config);
            configuration.ConfigSourceText = config.ToJson();
            return configuration;
        }

    }
}
