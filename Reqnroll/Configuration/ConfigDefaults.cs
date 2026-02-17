using System;
using Reqnroll.BindingSkeletons;

namespace Reqnroll.Configuration
{
// ReSharper disable RedundantNameQualifier
    public static class ConfigDefaults
    {
        public const string FeatureLanguage = "en-US";
        public const string ToolLanguage = "";

        public const string UnitTestProviderName = "nunit";

        public const bool DetectAmbiguousMatches = true;
        public const bool StopAtFirstError = false;
        public const MissingOrPendingStepsOutcome MissingOrPendingStepsOutcome = Reqnroll.Configuration.MissingOrPendingStepsOutcome.Pending;

        public const bool TraceSuccessfulSteps = true;
        public const bool TraceTimings = false;
        public const string MinTracedDuration = "0:0:0.1";
        public static TimeSpan MinTracedDurationAsTimeSpan { get; } = TimeSpan.Parse(MinTracedDuration);
        public const StepDefinitionSkeletonStyle StepDefinitionSkeletonStyle = Reqnroll.BindingSkeletons.StepDefinitionSkeletonStyle.CucumberExpressionAttribute;

        public const ObsoleteBehavior ObsoleteBehavior = Configuration.ObsoleteBehavior.Warn;
        public const bool ColoredOutput = false;

        public const bool AllowDebugGeneratedFiles = false;
        public const bool AllowRowTests = true;
        public const string GeneratorPath = null;
        public const bool DisableFriendlyTestNames = false;

        public static readonly string[] AddNonParallelizableMarkerForTags = Array.Empty<string>();
    }
// ReSharper restore RedundantNameQualifier
}