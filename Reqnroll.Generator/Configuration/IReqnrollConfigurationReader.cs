using Reqnroll.Generator.Interfaces;

namespace Reqnroll.Generator.Configuration
{
    public interface IReqnrollConfigurationReader
    {
        ReqnrollConfigurationHolder ReadConfiguration();
    }
}