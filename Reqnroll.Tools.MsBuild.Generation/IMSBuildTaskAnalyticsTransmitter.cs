using System.Threading.Tasks;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IMSBuildTaskAnalyticsTransmitter
    {
        Task TryTransmitProjectCompilingEventAsync();
    }
}
