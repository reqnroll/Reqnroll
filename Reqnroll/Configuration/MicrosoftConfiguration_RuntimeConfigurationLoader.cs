using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Reqnroll.Configuration.JsonConfig;
using SpecFlow.Internal.Json;

namespace Reqnroll.Configuration
{
    public class MicrosoftConfiguration_RuntimeConfigurationLoader : IMS_ConfigurationLoader
     {
        private readonly string _jsonConfigurationFilePath;
        private readonly IConfigurationManager _configurationManager;

        public MicrosoftConfiguration_RuntimeConfigurationLoader(IReqnrollJsonLocator reqnrollJsonLocator, IConfigurationManager configurationManager)
        {
            if (configurationManager == null) throw new ArgumentNullException(nameof(configurationManager), "Microsoft.Extensions.Configuration configurationManager cannot be null.");

            configurationManager.AddEnvironmentVariables("DOTNET_");
            string envName = configurationManager["Environment"];

            _jsonConfigurationFilePath = reqnrollJsonLocator.GetReqnrollJsonFilePath();


            if (_jsonConfigurationFilePath != null)
            {
                var pathWithoutExt = Path.Combine(Path.GetDirectoryName(_jsonConfigurationFilePath),Path.GetFileNameWithoutExtension(_jsonConfigurationFilePath));
                var envOverridePath = $"{pathWithoutExt}.{envName}.json";
                configurationManager.AddJsonFile(_jsonConfigurationFilePath, optional: true, reloadOnChange: false);
                configurationManager.AddJsonFile(envOverridePath, optional: true, reloadOnChange: false);
            }
            configurationManager.AddEnvironmentVariables(prefix: "REQNROLL__");
            _configurationManager = configurationManager;
            //configurationManager.Build();
            // Build not necessary as IConfigurationManager automatically rebuilds when sources are changed
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
