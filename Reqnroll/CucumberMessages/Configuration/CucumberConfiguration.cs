using Reqnroll.BoDi;
using Reqnroll.CommonModels;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Reqnroll.CucumberMessages.Configuration
{
    /// <summary>
    /// This class is responsible for determining the configuration of the Cucumber Messages subsystem.
    /// It is wired into the object container as a singleton and is a dependency of the PubSub classes.
    /// 
    /// When any consumer of this class asks for one of the properties of ICucumberConfiguration,
    /// the class will resolve the configuration (only once).
    /// 
    /// A default configuration is provided (by DefaultConfigurationSource). 
    /// It is supplemented by one or more profiles from the configuration file. (ConfigFile_ConfigurationSource)
    /// Then Environmment Variable Overrides are applied.
    /// </summary>
    public class CucumberConfiguration : ICucumberMessagesConfiguration
    {
        public static ICucumberMessagesConfiguration Current { get; private set; }
        public bool Enabled => _enablementOverrideFlag && _resolvedConfiguration.Value.Enabled;
        public string OutputFilePath => _resolvedConfiguration.Value.OutputFilePath;
        public IDGenerationStyle IDGenerationStyle => _resolvedConfiguration.Value.IDGenerationStyle;

        private readonly IObjectContainer _objectContainer;
        private Lazy<ITraceListener> _traceListenerLazy;
        private IEnvironmentWrapper _environmentWrapper;
        private IReqnrollJsonLocator _reqnrollJsonLocator;
        private Lazy<ResolvedConfiguration> _resolvedConfiguration;
        private bool _enablementOverrideFlag = true;

        public CucumberConfiguration(IObjectContainer objectContainer, IEnvironmentWrapper environmentWrapper, IReqnrollJsonLocator configurationFileLocator)
        {
            _objectContainer = objectContainer;
            _traceListenerLazy = new Lazy<ITraceListener>(() => _objectContainer.Resolve<ITraceListener>());
            _environmentWrapper = environmentWrapper;
            _reqnrollJsonLocator = configurationFileLocator;
            _resolvedConfiguration = new Lazy<ResolvedConfiguration>(ResolveConfiguration);
            Current = this;
        }

        #region Override API
        public void SetEnabled(bool value)
        {
            _enablementOverrideFlag = value;
        }
        #endregion


        private ResolvedConfiguration ResolveConfiguration()
        {
            var config = ApplyHierarchicalConfiguration();
            var resolved = ApplyEnvironmentOverrides(config);

            // a final sanity check, the filename cannot be empty
            if (string.IsNullOrEmpty(resolved.OutputFilePath))
            {
                resolved.OutputFilePath = "./reqnroll_report.ndjson";
            }
            EnsureOutputDirectory(resolved);
            return resolved;
        }
        private ConfigurationDTO ApplyHierarchicalConfiguration()
        {
            var defaultConfigurationProvider = new DefaultConfigurationSource(_environmentWrapper);
            var fileBasedConfigurationProvider = new ConfigFile_ConfigurationSource(_reqnrollJsonLocator);

            ConfigurationDTO defaultConfig = defaultConfigurationProvider.GetConfiguration();
            ConfigurationDTO fileBasedConfig = fileBasedConfigurationProvider.GetConfiguration();
            defaultConfig = MergeConfigs(defaultConfig, fileBasedConfig);
            return defaultConfig;
        }

        private ResolvedConfiguration ApplyEnvironmentOverrides(ConfigurationDTO config)
        {
            var filePathValue = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_OUTPUT_FILEPATH_ENVIRONMENT_VARIABLE);
            var idGenStyleValue = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ID_GENERATION_STYLE_ENVIRONMENT_VARIABLE);

            var result = new ResolvedConfiguration()
            {
                Enabled = config.Enabled,
                OutputFilePath = config.OutputFilePath,
                IDGenerationStyle = config.IDGenerationStyle            };

            if (filePathValue is Success<string>)
                result.OutputFilePath = ((Success<string>)filePathValue).Result;

            if (idGenStyleValue is Success<string>)
                result.IDGenerationStyle = IdGenerationStyleEnumConverter.ParseIdGenerationStyle(((Success<string>)idGenStyleValue).Result);

            var enabledResult = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ENABLE_ENVIRONMENT_VARIABLE);
            result.Enabled = enabledResult is Success<string> ? Convert.ToBoolean(((Success<string>)enabledResult).Result) : result.Enabled;

            return result;
        }

        private ConfigurationDTO MergeConfigs(ConfigurationDTO rootConfig, ConfigurationDTO overridingConfig)
        {
            if (overridingConfig != null)
            {
                rootConfig.Enabled = overridingConfig.Enabled;
                rootConfig.OutputFilePath = overridingConfig.OutputFilePath ?? rootConfig.OutputFilePath;
                rootConfig.IDGenerationStyle = overridingConfig.IDGenerationStyle;

            }

            return rootConfig;
        }

        private void EnsureOutputDirectory(ResolvedConfiguration config)
        {

            if (!Directory.Exists(Path.GetDirectoryName(config.OutputFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(config.OutputFilePath));
            }
        }

    }
}

