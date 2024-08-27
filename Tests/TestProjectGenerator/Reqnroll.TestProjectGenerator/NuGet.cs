using System;
using System.IO;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator
{
    public class NuGet
    {
        protected readonly Folders _folders;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly NetFrameworkExecutableInvoker _netFrameworkExecutableInvoker;

        public NuGet(Folders folders, TestProjectFolders testProjectFolders, NetFrameworkExecutableInvoker netFrameworkExecutableInvoker)
        {
            _folders = folders;
            _testProjectFolders = testProjectFolders;
            _netFrameworkExecutableInvoker = netFrameworkExecutableInvoker;
        }

        public void Restore()
        {
            string commandLineArgs = $"restore {_testProjectFolders.SolutionFileName} -SolutionDirectory . -NoCache";
            Restore(commandLineArgs);
        }

        public void RestoreForProject(Project project)
        {
            if (project.ProjectFormat != ProjectFormat.Old)
            {
                throw new InvalidOperationException("Project format must be classic to use NuGet.exe for restoring packages");
            }

            string pathToPackagesConfig = Path.Combine(_testProjectFolders.PathToSolutionDirectory, project.Name, "packages.config");
            string commandLineArgs = $"restore \"{pathToPackagesConfig}\" -SolutionDirectory . -NoCache";
            Restore(commandLineArgs);
        }

        public void Restore(string parameters)
        {
            string processPath = GetPathToNuGetExe();
            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException("NuGet.exe could not be found! Is the version number correct?", processPath);
            }

            var processResult = _netFrameworkExecutableInvoker.InvokeExecutable(processPath, parameters);

            if (processResult.ExitCode > 0)
            {
                throw new Exception("NuGet restore failed - rebuild solution to generate latest packages "
                                    + Environment.NewLine
                                    + $"{_testProjectFolders.PathToSolutionDirectory} {processPath} {parameters}"
                                    + Environment.NewLine
                                    + processResult.CombinedOutput);
            }
        }

        protected virtual string GetPathToNuGetExe()
        {
            return Path.Combine(_folders.SystemGlobalNuGetPackages, "NuGet.CommandLine".ToLower(), "6.11.0", "tools", "NuGet.exe");
        }
    }
}
