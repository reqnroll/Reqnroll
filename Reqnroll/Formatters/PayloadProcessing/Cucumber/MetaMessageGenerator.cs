using Io.Cucumber.Messages.Types;
using Reqnroll.EnvironmentAccess;

namespace Reqnroll.Formatters.PayloadProcessing.Cucumber;

public class MetaMessageGenerator(IEnvironmentInfoProvider environmentInfoProvider, IBuildMetadataProvider buildMetadataProvider, ICucumberMessageFactory cucumberMessageFactory) : IMetaMessageGenerator
{
    public Meta GenerateMetaMessage()
    {
        var buildMetaData = buildMetadataProvider.GetBuildMetadata() ?? BuildMetadata.Unknown;
        var reqnrollVersion = environmentInfoProvider.GetReqnrollVersion();
        var netCoreVersion = environmentInfoProvider.GetNetCoreVersion();
        var osPlatform = environmentInfoProvider.GetOSPlatform();

        return cucumberMessageFactory.ToMeta(reqnrollVersion, netCoreVersion, osPlatform, buildMetaData);
    }
}