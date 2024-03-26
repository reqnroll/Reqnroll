using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Reqnroll.TestProjectGenerator
{
    public class MSBuildFinder
    {
        private const string MSBUILD_ENV_VAR = "REQNROLL_TEST_MSBUILD_PATH";
        private readonly IOutputWriter _outputWriter;

        public MSBuildFinder(IOutputWriter outputWriter)
        {
            _outputWriter = outputWriter;
        }

        /// <summary>
        /// Source: https://github.com/Microsoft/vswhere/wiki/Installing
        /// </summary>
        private string GetVsWherePath()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var vsWherePath = Path.Combine(programFiles, "Microsoft Visual Studio", "Installer", "vswhere.exe");
            return vsWherePath;
        }

        private string FindUsingVsWhere()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;

            var vsWherePath = GetVsWherePath();
            if (vsWherePath == null || !File.Exists(vsWherePath)) return null;

            // based on instructions at https://github.com/microsoft/vswhere/wiki/Find-MSBuild
            const string vsWhereMsBuildParameter = @"-latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe";
            var processResult = new ProcessHelper()
                .RunProcess(_outputWriter, ".", vsWherePath, vsWhereMsBuildParameter);

            var msBuildPath = 
                Regex.Split(processResult.CombinedOutput, @"\r?\n")
                .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));

            if (msBuildPath == null || !File.Exists(msBuildPath)) return null;

            return msBuildPath;
        }

        private string FindUsingEnvironmentVariable()
        {
            var fromEnv = Environment.GetEnvironmentVariable(MSBUILD_ENV_VAR);
            if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv.Trim();
            return null;
        }

        public string FindMSBuild()
        {
            return FindUsingVsWhere() ?? 
                   FindUsingEnvironmentVariable() ??
                   "msbuild";
        }
    }
}
