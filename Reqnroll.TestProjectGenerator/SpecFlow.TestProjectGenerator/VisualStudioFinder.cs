using System;
using System.IO;
using System.Linq;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class VisualStudioFinder
    {
        private const string VsWhereInstallationPathParameter = @"-latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -property installationPath";
        private const string VsWhereMsBuildParameter = @"-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe";
        private readonly Folders _folders;
        private readonly IOutputWriter _outputWriter;

        public VisualStudioFinder(Folders folders, IOutputWriter outputWriter)
        {
            _folders = folders;
            _outputWriter = outputWriter;
        }

        public string Find()
        {
            return ExecuteVsWhere(VsWhereInstallationPathParameter);
        }

        public string FindMSBuild()
        {
            return ExecuteVsWhere(VsWhereMsBuildParameter);
        }

        private string ExecuteVsWhere(string vsWhereParameters)
        {
            var vsWherePath = GetVsWherePath();

            if (!File.Exists(vsWherePath))
            {
                vsWherePath = Path.Combine(_folders.GlobalPackages, "vswhere", "2.7.1", "tools", "vswhere.exe");

                if (!File.Exists(vsWherePath))
                {
                    throw new FileNotFoundException("vswhere can not be found! Is the version number correct?", vsWherePath);
                }
            }

            var ph = new ProcessHelper();
            var processResult = ph.RunProcess(_outputWriter, ".", vsWherePath, vsWhereParameters);

            var lines = processResult.CombinedOutput.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            if (!lines.Any())
            {
                throw new Exception($"vswhere didn't return something: {processResult.CombinedOutput}");
            }

            return lines.First();
        }

        /// <summary>
        /// Starting with Visual Studio 15.2 (26418.1 Preview) vswhere.exe is installed in
        /// %ProgramFiles(x86)%\Microsoft Visual Studio\Installer (use %ProgramFiles% in a 32-bit program prior to Windows 10).
        /// This is a fixed location that will be maintained.
        /// Source: https://github.com/Microsoft/vswhere/wiki/Installing
        /// </summary>
        private string GetVsWherePath()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var vsWherePath = Path.Combine(programFiles, "Microsoft Visual Studio", "Installer", "vswhere.exe");
            return vsWherePath;
        }
    }
}