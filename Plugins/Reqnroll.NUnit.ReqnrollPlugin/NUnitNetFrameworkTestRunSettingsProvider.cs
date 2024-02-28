using System.IO;
using Reqnroll.Plugins;
using Reqnroll.TestFramework;

namespace Reqnroll.NUnit.ReqnrollPlugin
{
    public class NUnitNetFrameworkTestRunSettingsProvider : ITestRunSettingsProvider
    {
        private readonly IReqnrollPath _reqnrollPath;

        public NUnitNetFrameworkTestRunSettingsProvider(IReqnrollPath reqnrollPath)
        {
            _reqnrollPath = reqnrollPath;
        }

        public string GetTestDirectory() => Path.GetDirectoryName(_reqnrollPath.GetPathToReqnrollDll());
    }
}
