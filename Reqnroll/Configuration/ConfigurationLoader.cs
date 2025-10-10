using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Reqnroll.BindingSkeletons;
using Reqnroll.Configuration.JsonConfig;
using Reqnroll.PlatformCompatibility;
using Reqnroll.Tracing;

namespace Reqnroll.Configuration
{
    public class ConfigurationLoader : IConfigurationLoader
    {
        //private readonly ObjectContainer _objectContainer;
        private readonly JsonConfigurationLoader _jsonConfigurationLoader;
        private readonly IReqnrollJsonLocator _reqnrollJsonLocator;

        public ConfigurationLoader(IReqnrollJsonLocator reqnrollJsonLocator)
        {
            _reqnrollJsonLocator = reqnrollJsonLocator;
            _jsonConfigurationLoader = new JsonConfigurationLoader();
        }

        private static CultureInfo DefaultFeatureLanguage => CultureInfo.GetCultureInfo(ConfigDefaults.FeatureLanguage);

        private static CultureInfo DefaultToolLanguage => CultureInfoHelper.GetCultureInfo(ConfigDefaults.FeatureLanguage);

        private static CultureInfo DefaultBindingCulture => null;
        private static string DefaultUnitTestProvider => ConfigDefaults.UnitTestProviderName;
        private static bool DefaultDetectAmbiguousMatches => ConfigDefaults.DetectAmbiguousMatches;
        private static bool DefaultStopAtFirstError => ConfigDefaults.StopAtFirstError;

        private static MissingOrPendingStepsOutcome DefaultMissingOrPendingStepsOutcome => ConfigDefaults.MissingOrPendingStepsOutcome;

        private static bool DefaultTraceSuccessfulSteps => ConfigDefaults.TraceSuccessfulSteps;
        private static bool DefaultTraceTimings => ConfigDefaults.TraceTimings;
        private static TimeSpan DefaultMinTracedDuration => ConfigDefaults.MinTracedDurationAsTimeSpan;

        private static StepDefinitionSkeletonStyle DefaultStepDefinitionSkeletonStyle => ConfigDefaults.StepDefinitionSkeletonStyle;
        private static List<string> DefaultAdditionalStepAssemblies => new List<string>();
        private static bool DefaultAllowDebugGeneratedFiles => ConfigDefaults.AllowDebugGeneratedFiles;
        private static bool DefaultAllowRowTests => ConfigDefaults.AllowRowTests;
        public static string DefaultGeneratorPath => ConfigDefaults.GeneratorPath;

        public static string[] DefaultAddNonParallelizableMarkerForTags => ConfigDefaults.AddNonParallelizableMarkerForTags;

        public static bool DefaultDisableFriendlyTestNames => ConfigDefaults.DisableFriendlyTestNames;

        public static ObsoleteBehavior DefaultObsoleteBehavior => ConfigDefaults.ObsoleteBehavior;

        public static bool DefaultColoredOutput => ConfigDefaults.ColoredOutput;

        public bool HasJsonConfig
        {
            get
            {
                var reqnrollJsonFile = _reqnrollJsonLocator.GetReqnrollJsonFilePath();
                return File.Exists(reqnrollJsonFile);
            }
        }

        public ReqnrollConfiguration Load(ReqnrollConfiguration reqnrollConfiguration, IReqnrollConfigurationHolder reqnrollConfigurationHolder)
        {
            if (!reqnrollConfigurationHolder.HasConfiguration)
                return GetDefault();

            return LoadFromConfigSource(reqnrollConfiguration, reqnrollConfigurationHolder);
        }

        protected virtual ReqnrollConfiguration LoadFromConfigSource(ReqnrollConfiguration reqnrollConfiguration, IReqnrollConfigurationHolder reqnrollConfigurationHolder)
        {
            return reqnrollConfigurationHolder.ConfigSource switch
            {
                ConfigSource.Default => GetDefault(),
                ConfigSource.Json => LoadJson(reqnrollConfiguration, reqnrollConfigurationHolder.Content),
                ConfigSource.AppConfig => GetAppConfigStub(reqnrollConfiguration, reqnrollConfigurationHolder.Content),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private ReqnrollConfiguration GetAppConfigStub(ReqnrollConfiguration reqnrollConfiguration, string content)
        {
            var configuration = GetDefault();
            configuration.ConfigSource = ConfigSource.AppConfig;
            configuration.ConfigSourceText = content;
            return configuration;
        }

        public ReqnrollConfiguration Load(ReqnrollConfiguration reqnrollConfiguration)
        {
            if (HasJsonConfig)
                return LoadJson(reqnrollConfiguration);

            return GetDefault();
        }

        public void TraceConfigSource(ITraceListener traceListener, ReqnrollConfiguration reqnrollConfiguration)
        {
            switch (reqnrollConfiguration.ConfigSource)
            {
                case ConfigSource.Default:
                    traceListener.WriteToolOutput("Using default config");
                    break;
                case ConfigSource.AppConfig:
                    traceListener.WriteToolOutput("Using app.config");
                    break;
                case ConfigSource.Json:
                    traceListener.WriteToolOutput("Using reqnroll.json");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public static ReqnrollConfiguration GetDefault()
        {
            return new ReqnrollConfiguration(ConfigSource.Default,
                new DependencyConfigurationCollection(),
                new DependencyConfigurationCollection(),
                DefaultFeatureLanguage,
                DefaultBindingCulture,
                DefaultStopAtFirstError,
                DefaultMissingOrPendingStepsOutcome,
                DefaultTraceSuccessfulSteps,
                DefaultTraceTimings,
                DefaultMinTracedDuration,
                DefaultStepDefinitionSkeletonStyle,
                DefaultAdditionalStepAssemblies,
                DefaultAllowDebugGeneratedFiles,
                DefaultAllowRowTests,
                DefaultAddNonParallelizableMarkerForTags,
                DefaultDisableFriendlyTestNames,
                DefaultObsoleteBehavior,
                DefaultColoredOutput
                );
        }

        private ReqnrollConfiguration LoadJson(ReqnrollConfiguration reqnrollConfiguration)
        {
            var jsonContent = File.ReadAllText(_reqnrollJsonLocator.GetReqnrollJsonFilePath());

            return LoadJson(reqnrollConfiguration, jsonContent);
        }

        private ReqnrollConfiguration LoadJson(ReqnrollConfiguration reqnrollConfiguration, string jsonContent)
        {
            return _jsonConfigurationLoader.LoadJson(reqnrollConfiguration, jsonContent);
        }
    }
}
