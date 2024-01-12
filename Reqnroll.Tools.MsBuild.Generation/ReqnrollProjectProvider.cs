using System.IO;
using Reqnroll.Generator.Project;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class ReqnrollProjectProvider : IReqnrollProjectProvider
    {
        private readonly IMSBuildProjectReader _msbuildProjectReader;
        private readonly ReqnrollProjectInfo _reqnrollProjectInfo;

        public ReqnrollProjectProvider(IMSBuildProjectReader msbuildProjectReader, ReqnrollProjectInfo reqnrollProjectInfo)
        {
            _msbuildProjectReader = msbuildProjectReader;
            _reqnrollProjectInfo = reqnrollProjectInfo;
        }

        public ReqnrollProject GetReqnrollProject()
        {
            var reqnrollProject = _msbuildProjectReader.LoadReqnrollProjectFromMsBuild(Path.GetFullPath(_reqnrollProjectInfo.ProjectPath), _reqnrollProjectInfo.RootNamespace);
            return reqnrollProject;
        }
    }
}
