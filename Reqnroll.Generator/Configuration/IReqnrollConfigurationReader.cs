using Reqnroll.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.Project;

namespace Reqnroll.Generator.Configuration
{
    public interface IReqnrollConfigurationReader
    {
        ReqnrollConfigurationHolder ReadConfiguration();
    }
}