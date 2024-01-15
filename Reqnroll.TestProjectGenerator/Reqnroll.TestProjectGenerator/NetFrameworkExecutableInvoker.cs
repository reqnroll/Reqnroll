using System.Runtime.InteropServices;

namespace Reqnroll.TestProjectGenerator
{
    public class NetFrameworkExecutableInvoker
    {
        private readonly ProcessHelper _processHelper;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;

        public NetFrameworkExecutableInvoker(ProcessHelper processHelper, TestProjectFolders testProjectFolders, IOutputWriter outputWriter)
        {
            _processHelper = processHelper;
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
        }

        public ProcessResult InvokeExecutable(string executablePath, string parameters)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return  _processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, executablePath, parameters);
            }

            return _processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "/usr/bin/mono", $"{executablePath} {parameters}");
        }
    }
}
