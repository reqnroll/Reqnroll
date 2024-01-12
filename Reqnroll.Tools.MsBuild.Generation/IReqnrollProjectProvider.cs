using Reqnroll.Generator.Project;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IReqnrollProjectProvider
    {
        ReqnrollProject GetReqnrollProject();
    }
}
