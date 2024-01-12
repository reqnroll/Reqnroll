using Reqnroll.Analytics;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class GenerateFeatureFileCodeBehindTaskConfiguration
    {
        public GenerateFeatureFileCodeBehindTaskConfiguration(IAnalyticsTransmitter overrideAnalyticsTransmitter, IFeatureFileCodeBehindGenerator overrideFeatureFileCodeBehindGenerator)
        {
            OverrideAnalyticsTransmitter = overrideAnalyticsTransmitter;
            OverrideFeatureFileCodeBehindGenerator = overrideFeatureFileCodeBehindGenerator;
        }

        public IAnalyticsTransmitter OverrideAnalyticsTransmitter { get; }

        public IFeatureFileCodeBehindGenerator OverrideFeatureFileCodeBehindGenerator { get; }
    }
}
