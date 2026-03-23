using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Reqnroll.TestProjectGenerator
{
    public class MSBuildFinder(IOutputWriter _outputWriter, ConfigurationDriver _configDriver)
    {
        private string FindUsingVS2026()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;

            // Finding paths, like
            //   C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe

            string GetVs2026Path(string edition)
            {
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                return Path.Combine(programFiles, "Microsoft Visual Studio", "18", edition);
            }

            var editions = new[] { "Enterprise", "Community", "Professional" };
            foreach (var vsPath in editions.Select(GetVs2026Path))
            {
                var msBuildPath = Path.Combine(vsPath, "MSBuild", "Current", "Bin", "MSBuild.exe");
                if (File.Exists(msBuildPath)) return msBuildPath;
            }
            return null;
        }

        private string FindUsingVsWhere()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;

            //Source: https://github.com/Microsoft/vswhere/wiki/Installing
            string GetVsWherePath()
            {
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                return Path.Combine(programFiles, "Microsoft Visual Studio", "Installer", "vswhere.exe");
            }

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

        private string FindUsingEnvironmentVariableAndConfig()
        {
            return _configDriver.MsBuildPath;
        }

        public string FindMSBuild()
        {
            return FindUsingEnvironmentVariableAndConfig() ??
                   FindUsingVS2026() ??
                   FindUsingVsWhere() ?? 
                   "msbuild";
        }
    }
}
