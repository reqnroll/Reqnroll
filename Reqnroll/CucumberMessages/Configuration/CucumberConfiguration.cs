using Reqnroll.BoDi;
using Reqnroll.CommonModels;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;

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

        public string FormatterConfiguration(string formatterName)
        {
            var config = _resolvedConfiguration.Value;
            if (config.Formatters.TryGetValue(formatterName, out var formatterConfig))
                return formatterConfig;
            else return String.Empty;
        }

        private ResolvedConfiguration ResolveConfiguration()
        {
            var config = ApplyHierarchicalConfiguration();
            var resolved = ApplyEnvironmentOverrides(config);

            return resolved;
        }
        private ResolvedConfiguration ApplyHierarchicalConfiguration()
        {
            var defaultConfigurationProvider = new DefaultConfigurationSource(_environmentWrapper);
            var fileBasedConfigurationProvider = new ConfigFile_ConfigurationSource(_reqnrollJsonLocator);

            var defaultConfigEntries = defaultConfigurationProvider.GetConfiguration();
            var fileBasedConfigEntries = fileBasedConfigurationProvider.GetConfiguration();

            foreach (var entry in fileBasedConfigEntries)
                defaultConfigEntries.Add(entry.Key, entry.Value);

            ResolvedConfiguration result = new() { Formatters = defaultConfigEntries, Enabled = defaultConfigEntries.Count > 0 };
            return result;
        }

        private ResolvedConfiguration ApplyEnvironmentOverrides(ResolvedConfiguration config)
        {
            //Debugger.Launch();

            var formatters = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_FORMATTERS_ENVIRONMENT_VARIABLE);
            // treating formatters as a json string containing an object, iterate over the properties and add them to the configuration, replacing all existing values;
            if (formatters is Success<string> formattersSuccess)
            {
                config.Formatters.Clear();
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                using JsonDocument formattersDoc = JsonDocument.Parse(formattersSuccess.Result, new JsonDocumentOptions()
                {
                    CommentHandling = JsonCommentHandling.Skip
                });
                var formattersEntry = formattersDoc.RootElement;
                foreach (JsonProperty jsonProperty in formattersEntry.EnumerateObject())
                {
                    config.Formatters[jsonProperty.Name] = jsonProperty.Value.GetRawText();
                }
            }

            var enabledVariable = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ENABLE_ENVIRONMENT_VARIABLE);
            var enabledVariableValue = enabledVariable is Success<string> ? Convert.ToBoolean(((Success<string>)enabledVariable).Result) : config.Enabled;
            config.Enabled = config.Enabled && enabledVariableValue;
            return config;
        }
    }
}

