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
        public string BaseDirectory => _resolvedConfiguration.Value.BaseDirectory;
        public string OutputDirectory => _resolvedConfiguration.Value.OutputDirectory;
        public string OutputFileName => _resolvedConfiguration.Value.OutputFileName;
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
            if (string.IsNullOrEmpty(resolved.OutputFileName))
            {
                resolved.OutputFileName = "reqnroll_report.ndjson";
                _traceListenerLazy.Value.WriteToolOutput($"WARNING: Cucumber Messages: Output filename was empty. Setting filename to {resolved.OutputFileName}");
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
            var baseOutDirValue = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_OUTPUT_BASE_DIRECTORY_ENVIRONMENT_VARIABLE);
            var relativePathValue = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_OUTPUT_RELATIVE_PATH_ENVIRONMENT_VARIABLE);
            var fileNameValue = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_OUTPUT_FILENAME_ENVIRONMENT_VARIABLE);
            var idGenStyleValue = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ID_GENERATION_STYLE_ENVIRONMENT_VARIABLE);

            var result = new ResolvedConfiguration()
            {
                Enabled = config.Enabled,
                BaseDirectory = config.BaseDirectory,
                OutputDirectory = config.OutputDirectory,
                OutputFileName = config.OutputFileName,
                IDGenerationStyle = config.IDGenerationStyle            };

            if (baseOutDirValue is Success<string>)
                result.BaseDirectory = ((Success<string>)baseOutDirValue).Result;

            if (relativePathValue is Success<string>)
                result.OutputDirectory = ((Success<string>)relativePathValue).Result;

            if (fileNameValue is Success<string>)
                result.OutputFileName = ((Success<string>)fileNameValue).Result;

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
                rootConfig.BaseDirectory = overridingConfig.BaseDirectory ?? rootConfig.BaseDirectory;
                rootConfig.OutputDirectory = overridingConfig.OutputDirectory ?? rootConfig.OutputDirectory;
                rootConfig.OutputFileName = overridingConfig.OutputFileName ?? rootConfig.OutputFileName;
                rootConfig.IDGenerationStyle = overridingConfig.IDGenerationStyle;

            }

            return rootConfig;
        }

        private void EnsureOutputDirectory(ResolvedConfiguration config)
        {

            if (!Directory.Exists(config.BaseDirectory))
            {
                Directory.CreateDirectory(config.BaseDirectory);
                Directory.CreateDirectory(config.OutputDirectory);
            }
            else if (!Directory.Exists(Path.Combine(config.BaseDirectory, config.OutputDirectory)))
            {
                Directory.CreateDirectory(Path.Combine(config.BaseDirectory, config.OutputDirectory));
            }
        }

    }
}

