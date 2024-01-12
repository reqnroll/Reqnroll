using System;
using System.Globalization;
using SpecFlow.Internal.Json;

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

            var jsonConfig = jsonContent.FromJson<JsonConfig>();

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
            var obsoleteBehavior = reqnrollConfiguration.ObsoleteBehavior;

            if (jsonConfig.Language != null)
            {
                if (jsonConfig.Language.Feature.IsNotNullOrWhiteSpace())
                {
                    featureLanguage = CultureInfo.GetCultureInfo(jsonConfig.Language.Feature);
                }
            }

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
                        containerRegistrationCollection.Add(runtimeDependency.ImplementationType, runtimeDependency.InterfaceType);
                    }
                }
            }

            if (jsonConfig.Generator != null)
            {
                allowDebugGeneratedFiles = jsonConfig.Generator.AllowDebugGeneratedFiles;
                allowRowTests = jsonConfig.Generator.AllowRowTests;
                addNonParallelizableMarkerForTags = jsonConfig.Generator.AddNonParallelizableMarkerForTags?.ToArray();
            }

            if (jsonConfig.Trace != null)
            {
                traceSuccessfulSteps = jsonConfig.Trace.TraceSuccessfulSteps;
                traceTimings = jsonConfig.Trace.TraceTimings;
                minTracedDuration = jsonConfig.Trace.MinTracedDuration;
                stepDefinitionSkeletonStyle = jsonConfig.Trace.StepDefinitionSkeletonStyle;
                coloredOutput = jsonConfig.Trace.ColoredOutput;
            }

            if (jsonConfig.StepAssemblies != null)
            {
                foreach (var stepAssemblyEntry in jsonConfig.StepAssemblies)
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
                obsoleteBehavior,
                coloredOutput
            );
        }
    }
}
