using System.IO;
using Reqnroll.Plugins;
using Reqnroll.TestFramework;

namespace Reqnroll.NUnit.ReqnrollPlugin
{
    public class NUnitNetFrameworkTestRunContext : ITestRunContext
    {
        private readonly IReqnrollPath _reqnrollPath;

        public NUnitNetFrameworkTestRunContext(IReqnrollPath reqnrollPath)
        {
            _reqnrollPath = reqnrollPath;
        }

        public string GetTestDirectory() => Path.GetDirectoryName(_reqnrollPath.GetPathToReqnrollDll());
    }
}
