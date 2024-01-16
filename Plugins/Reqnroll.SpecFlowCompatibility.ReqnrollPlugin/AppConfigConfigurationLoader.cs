using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using BoDi;
using Reqnroll.BindingSkeletons;
using Reqnroll.Configuration;
using Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.AppConfig;
using Reqnroll.Tracing;

namespace Reqnroll.SpecFlowCompatibility.ReqnrollPlugin;

public class AppConfigConfigurationLoader
{
    public bool HasAppConfig => ConfigurationManager.GetSection("specFlow") != null;

    public ReqnrollConfiguration LoadAppConfig(ReqnrollConfiguration reqnrollConfiguration, ConfigurationSectionHandler configSection)
    {
        if (configSection == null) throw new ArgumentNullException(nameof(configSection));

        ContainerRegistrationCollection runtimeContainerRegistrationCollection = reqnrollConfiguration.CustomDependencies;
        ContainerRegistrationCollection generatorContainerRegistrationCollection = reqnrollConfiguration.GeneratorCustomDependencies;
        CultureInfo featureLanguage = reqnrollConfiguration.FeatureLanguage;
        CultureInfo bindingCulture = reqnrollConfiguration.BindingCulture;
        bool stopAtFirstError = reqnrollConfiguration.StopAtFirstError;
        MissingOrPendingStepsOutcome missingOrPendingStepsOutcome = reqnrollConfiguration.MissingOrPendingStepsOutcome;
        bool traceSuccessfulSteps = reqnrollConfiguration.TraceSuccessfulSteps;
        bool traceTimings = reqnrollConfiguration.TraceTimings;
        TimeSpan minTracedDuration = reqnrollConfiguration.MinTracedDuration;
        StepDefinitionSkeletonStyle stepDefinitionSkeletonStyle = reqnrollConfiguration.StepDefinitionSkeletonStyle;
        List<string> additionalStepAssemblies = reqnrollConfiguration.AdditionalStepAssemblies;
        ObsoleteBehavior obsoleteBehavior = reqnrollConfiguration.ObsoleteBehavior;
        bool coloredOutput = reqnrollConfiguration.ColoredOutput;

        bool allowRowTests = reqnrollConfiguration.AllowRowTests;
        bool allowDebugGeneratedFiles = reqnrollConfiguration.AllowDebugGeneratedFiles;
        string[] addNonParallelizableMarkerForTags = reqnrollConfiguration.AddNonParallelizableMarkerForTags;


        if (IsSpecified(configSection.Language))
        {
            featureLanguage = CultureInfo.GetCultureInfo(configSection.Language.Feature);
        }

        if (IsSpecified(configSection.BindingCulture))
        {
            bindingCulture = CultureInfo.GetCultureInfo(configSection.BindingCulture.Name);
        }

        if (IsSpecified(configSection.Runtime))
        {
            stopAtFirstError = configSection.Runtime.StopAtFirstError;
            missingOrPendingStepsOutcome = configSection.Runtime.MissingOrPendingStepsOutcome;
            obsoleteBehavior = configSection.Runtime.ObsoleteBehavior;

            if (IsSpecified(configSection.Runtime.Dependencies))
            {
                runtimeContainerRegistrationCollection = configSection.Runtime.Dependencies;
            }
        }

        if (IsSpecified((configSection.Generator)))
        {
            allowDebugGeneratedFiles = configSection.Generator.AllowDebugGeneratedFiles;
            allowRowTests = configSection.Generator.AllowRowTests;

            if (IsSpecified(configSection.Generator.AddNonParallelizableMarkerForTags))
            {
                addNonParallelizableMarkerForTags = configSection.Generator.AddNonParallelizableMarkerForTags.Select(i => i.Value).ToArray();
            }

            if (IsSpecified(configSection.Generator.Dependencies))
            {
                generatorContainerRegistrationCollection = configSection.Generator.Dependencies;
            }
        }

        if (IsSpecified(configSection.Trace))
        {
            if (!string.IsNullOrEmpty(configSection.Trace.Listener)) // backwards compatibility
            {
                runtimeContainerRegistrationCollection.Add(configSection.Trace.Listener, typeof(ITraceListener).AssemblyQualifiedName);
            }

            traceSuccessfulSteps = configSection.Trace.TraceSuccessfulSteps;
            traceTimings = configSection.Trace.TraceTimings;
            minTracedDuration = configSection.Trace.MinTracedDuration;
            stepDefinitionSkeletonStyle = configSection.Trace.StepDefinitionSkeletonStyle;
            coloredOutput = configSection.Trace.ColoredOutput;
        }

        foreach (var element in configSection.StepAssemblies)
        {
            var assemblyName = ((StepAssemblyConfigElement)element).Assembly;
            additionalStepAssemblies.Add(assemblyName);
        }

        return new ReqnrollConfiguration(ConfigSource.AppConfig,
                                         runtimeContainerRegistrationCollection,
                                         generatorContainerRegistrationCollection,
                                         featureLanguage,
                                         bindingCulture,
                                         stopAtFirstError,
                                         missingOrPendingStepsOutcome,
                                         traceSuccessfulSteps,
                                         traceTimings,
                                         minTracedDuration,
                                         stepDefinitionSkeletonStyle,
                                         additionalStepAssemblies,
                                         allowDebugGeneratedFiles,
                                         allowRowTests,
                                         addNonParallelizableMarkerForTags,
                                         obsoleteBehavior,
                                         coloredOutput
        );
    }

    private bool IsSpecified(ConfigurationElement configurationElement)
    {
        return configurationElement != null && configurationElement.ElementInformation.IsPresent;
    }
}