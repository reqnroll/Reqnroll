using System;
using System.Globalization;
using System.Text.Json;

namespace Reqnroll.Configuration.JsonConfig
{
    public class JsonConfigurationLoader
    {
        public ReqnrollConfiguration LoadJson(ReqnrollConfiguration reqnrollConfiguration, string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentNullException(nameof(jsonContent));
            }

            var jsonConfig = JsonSerializer.Deserialize(jsonContent, JsonConfigurationSourceGenerator.Default.JsonConfig);

            var containerRegistrationCollection = reqnrollConfiguration.CustomDependencies;
            var generatorContainerRegistrationCollection = reqnrollConfiguration.GeneratorCustomDependencies;
            var featureLanguage = reqnrollConfiguration.FeatureLanguage;
            var bindingCulture = reqnrollConfiguration.BindingCulture;
            bool stopAtFirstError = reqnrollConfiguration.StopAtFirstError;
            var missingOrPendingStepsOutcome = reqnrollConfiguration.MissingOrPendingStepsOutcome;
            bool traceSuccessfulSteps = reqnrollConfiguration.TraceSuccessfulSteps;
            bool traceTimings = reqnrollConfiguration.TraceTimings;
            var minTracedDuration = reqnrollConfiguration.MinTracedDuration;
            bool coloredOutput = reqnrollConfiguration.ColoredOutput;
            var stepDefinitionSkeletonStyle = reqnrollConfiguration.StepDefinitionSkeletonStyle;
            var additionalStepAssemblies = reqnrollConfiguration.AdditionalStepAssemblies;
            bool allowRowTests = reqnrollConfiguration.AllowRowTests;
            bool allowDebugGeneratedFiles = reqnrollConfiguration.AllowDebugGeneratedFiles;
            var addNonParallelizableMarkerForTags = reqnrollConfiguration.AddNonParallelizableMarkerForTags;
            bool disableFriendlyTestNames = reqnrollConfiguration.DisableFriendlyTestNames;
            var obsoleteBehavior = reqnrollConfiguration.ObsoleteBehavior;

            if (jsonConfig.Language != null)
            {
                if (jsonConfig.Language.Feature.IsNotNullOrWhiteSpace())
                {
                    featureLanguage = CultureInfo.GetCultureInfo(jsonConfig.Language.Feature);
                }
                if (jsonConfig.Language.Binding.IsNotNullOrWhiteSpace())
                {
                    bindingCulture = CultureInfo.GetCultureInfo(jsonConfig.Language.Binding);
                }
            }

            // legacy config
            if (jsonConfig.BindingCulture != null)
            {
                if (jsonConfig.BindingCulture.Name.IsNotNullOrWhiteSpace())
                {
                    bindingCulture = CultureInfo.GetCultureInfo(jsonConfig.BindingCulture.Name);
                }
            }

            if (jsonConfig.Runtime != null)
            {
                missingOrPendingStepsOutcome = jsonConfig.Runtime.MissingOrPendingStepsOutcome;
                stopAtFirstError = jsonConfig.Runtime.StopAtFirstError;
                obsoleteBehavior = jsonConfig.Runtime.ObsoleteBehavior;

                if (jsonConfig.Runtime.Dependencies != null)
                {
                    foreach (var runtimeDependency in jsonConfig.Runtime.Dependencies)
                    {
                        containerRegistrationCollection.Add(runtimeDependency.ImplementationType, runtimeDependency.InterfaceType, runtimeDependency.Name);
                    }
                }
            }

            if (jsonConfig.Generator != null)
            {
                allowDebugGeneratedFiles = jsonConfig.Generator.AllowDebugGeneratedFiles;
                allowRowTests = jsonConfig.Generator.AllowRowTests;
                addNonParallelizableMarkerForTags = jsonConfig.Generator.AddNonParallelizableMarkerForTags?.ToArray();
                disableFriendlyTestNames = jsonConfig.Generator.DisableFriendlyTestNames;
            }

            if (jsonConfig.Trace != null)
            {
                // legacy config
                traceSuccessfulSteps = jsonConfig.Trace.TraceSuccessfulSteps;
                // legacy config
                traceTimings = jsonConfig.Trace.TraceTimings;
                // legacy config
                minTracedDuration = jsonConfig.Trace.MinTracedDuration;
                stepDefinitionSkeletonStyle = jsonConfig.Trace.StepDefinitionSkeletonStyle;
                coloredOutput = jsonConfig.Trace.ColoredOutput;
            }

            // legacy config
            if (jsonConfig.StepAssemblies != null)
            {
                foreach (var stepAssemblyEntry in jsonConfig.StepAssemblies)
                {
                    additionalStepAssemblies.Add(stepAssemblyEntry.Assembly);
                }
            }

            if (jsonConfig.BindingAssemblies != null)
            {
                foreach (var stepAssemblyEntry in jsonConfig.BindingAssemblies)
                {
                    additionalStepAssemblies.Add(stepAssemblyEntry.Assembly);
                }
            }

            return new ReqnrollConfiguration(
                ConfigSource.Json,
                containerRegistrationCollection,
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
                disableFriendlyTestNames,
                obsoleteBehavior,
                coloredOutput
            )
            {
                ConfigSourceText = jsonContent
            };
        }
    }
}
