using Reqnroll.Tracing;

namespace Reqnroll.Configuration
{
    public interface IConfigurationLoader
    {
        ReqnrollConfiguration Load(ReqnrollConfiguration reqnrollConfiguration, IReqnrollConfigurationHolder reqnrollConfigurationHolder);

        ReqnrollConfiguration Load(ReqnrollConfiguration reqnrollConfiguration);

        void TraceConfigSource(ITraceListener traceListener, ReqnrollConfiguration reqnrollConfiguration);
    }
}
