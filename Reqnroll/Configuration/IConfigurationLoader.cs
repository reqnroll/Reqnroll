using Reqnroll.Tracing;

namespace Reqnroll.Configuration
{
    public interface IConfigurationLoader
    {
        ReqnrollConfiguration Load(ReqnrollConfiguration reqnrollConfiguration, IReqnrollConfigurationHolder reqnrollConfigurationHolder);

        ReqnrollConfiguration Load(ReqnrollConfiguration reqnrollConfiguration);

        ReqnrollConfiguration Update(ReqnrollConfiguration reqnrollConfiguration, ConfigurationSectionHandler reqnrollConfigSection);

        void TraceConfigSource(ITraceListener traceListener, ReqnrollConfiguration reqnrollConfiguration);
    }
}
