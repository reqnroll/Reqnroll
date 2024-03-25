using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Reqnroll.Configuration;
using Reqnroll.Tracing;

namespace Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.AppConfig;

public class AppConfigConfigurationLoader
{
    public bool HasAppConfig => ConfigurationManager.GetSection("specFlow") != null;

    public void UpdateFromAppConfig(ReqnrollConfiguration reqnrollConfiguration, ConfigurationSectionHandler configSection)
    {
        if (configSection == null) throw new ArgumentNullException(nameof(configSection));

        if (IsSpecified(configSection.Language))
        {
            reqnrollConfiguration.FeatureLanguage = CultureInfo.GetCultureInfo(configSection.Language.Feature);
        }

        if (IsSpecified(configSection.BindingCulture))
        {
            reqnrollConfiguration.BindingCulture = CultureInfo.GetCultureInfo(configSection.BindingCulture.Name);
        }

        if (IsSpecified(configSection.Runtime))
        {
            reqnrollConfiguration.StopAtFirstError = configSection.Runtime.StopAtFirstError;
            reqnrollConfiguration.MissingOrPendingStepsOutcome = configSection.Runtime.MissingOrPendingStepsOutcome;
            reqnrollConfiguration.ObsoleteBehavior = configSection.Runtime.ObsoleteBehavior;

            if (IsSpecified(configSection.Runtime.Dependencies))
            {
                reqnrollConfiguration.CustomDependencies = configSection.Runtime.Dependencies.ToDependencyConfigurationCollection();
            }
        }

        if (IsSpecified((configSection.Generator)))
        {
            reqnrollConfiguration.AllowDebugGeneratedFiles = configSection.Generator.AllowDebugGeneratedFiles;
            reqnrollConfiguration.AllowRowTests = configSection.Generator.AllowRowTests;

            if (IsSpecified(configSection.Generator.AddNonParallelizableMarkerForTags))
            {
                reqnrollConfiguration.AddNonParallelizableMarkerForTags = configSection.Generator.AddNonParallelizableMarkerForTags.Select(i => i.Value).ToArray();
            }

            if (IsSpecified(configSection.Generator.Dependencies))
            {
                reqnrollConfiguration.GeneratorCustomDependencies = configSection.Generator.Dependencies.ToDependencyConfigurationCollection();
            }
        }

        if (IsSpecified(configSection.Trace))
        {
            if (!string.IsNullOrEmpty(configSection.Trace.Listener)) // backwards compatibility
            {
                reqnrollConfiguration.CustomDependencies.Add(configSection.Trace.Listener, typeof(ITraceListener).AssemblyQualifiedName);
            }

            reqnrollConfiguration.TraceSuccessfulSteps = configSection.Trace.TraceSuccessfulSteps;
            reqnrollConfiguration.TraceTimings = configSection.Trace.TraceTimings;
            reqnrollConfiguration.MinTracedDuration = configSection.Trace.MinTracedDuration;
            reqnrollConfiguration.StepDefinitionSkeletonStyle = configSection.Trace.StepDefinitionSkeletonStyle;
            reqnrollConfiguration.ColoredOutput = configSection.Trace.ColoredOutput;
        }

        foreach (var element in configSection.StepAssemblies)
        {
            var assemblyName = ((StepAssemblyConfigElement)element).Assembly;
            reqnrollConfiguration.AdditionalStepAssemblies.Add(assemblyName);
        }

        reqnrollConfiguration.ConfigSource = ConfigSource.AppConfig;
    }

    private bool IsSpecified(ConfigurationElement configurationElement)
    {
        return configurationElement != null && configurationElement.ElementInformation.IsPresent;
    }
}