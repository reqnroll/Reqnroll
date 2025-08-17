using System.Threading.Tasks;
using Reqnroll.CommonModels;

namespace Reqnroll.Analytics
{
    public interface IAnalyticsTransmitter
    {
        bool IsEnabled { get; }

        Task<IResult> TransmitReqnrollProjectCompilingEventAsync(ReqnrollProjectCompilingEvent projectCompilingEvent);
        Task<IResult> TransmitReqnrollProjectRunningEventAsync(ReqnrollProjectRunningEvent projectRunningEvent);
        Task<IResult> TransmitReqnrollFeatureUseEventAsync(ReqnrollFeatureUseEvent featureUseEvent);
    }
}
