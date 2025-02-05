using System;
using Reqnroll.TestProjectGenerator.Driver;

namespace Reqnroll.TestProjectGenerator
{
    public class Compiler
    {
        private readonly MSBuildFinder _msBuildFinder;
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;

        public Compiler(MSBuildFinder msBuildFinder, TestProjectFolders testProjectFolders, IOutputWriter outputWriter)
        {
            _msBuildFinder = msBuildFinder;
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
        }

        public CompileResult Run(BuildTool buildTool, bool? treatWarningsAsErrors, string logLevel)
        {
            switch (buildTool)
            {
                case BuildTool.MSBuild:
                    return CompileWithMSBuild(treatWarningsAsErrors, logLevel);
                case BuildTool.DotnetBuild:
                    return CompileWithDotnetBuild(treatWarningsAsErrors, logLevel);
                case BuildTool.DotnetMSBuild:
                    return CompileWithDotnetMSBuild(treatWarningsAsErrors, logLevel);
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildTool), buildTool, null);
            }
        }

        private CompileResult CompileWithMSBuild(bool? treatWarningsAsErrors, string logLevel)
        {
            var msBuildPath = _msBuildFinder.FindMSBuild();

            _outputWriter.WriteLine($"Invoke MsBuild from {msBuildPath}");

            var processHelper = new ProcessHelper();
            string argumentsFormat = $"{GetWarningAsErrorParameter(treatWarningsAsErrors)} -restore -binaryLogger:;ProjectImports=None -nodeReuse:false -v:{logLevel ?? "m"} \"{_testProjectFolders.PathToSolutionFile}\"";

            var msBuildProcess = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, msBuildPath, argumentsFormat);

            return new CompileResult(msBuildProcess.ExitCode, msBuildProcess.CombinedOutput);
        }

        private string GetWarningAsErrorParameter(bool? treatWarningsAsErrors)
        {
            return treatWarningsAsErrors is true ? "-warnaserror" : "";
        }

        private CompileResult CompileWithDotnetBuild(bool? treatWarningsAsErrors, string logLevel)
        {
            _outputWriter.WriteLine("Invoking dotnet build");

            var processHelper = new ProcessHelper();

            string argumentsFormat = $"build {GetWarningAsErrorParameter(treatWarningsAsErrors)} -nologo -v:{logLevel ?? "m"} \"{_testProjectFolders.PathToSolutionFile}\"";
            var dotnetBuildProcessResult = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "dotnet", argumentsFormat);

            return new CompileResult(dotnetBuildProcessResult.ExitCode, dotnetBuildProcessResult.CombinedOutput);
        }

        private CompileResult CompileWithDotnetMSBuild(bool? treatWarningsAsErrors, string logLevel)
        {
            _outputWriter.WriteLine($"Invoking dotnet msbuild");

            var processHelper = new ProcessHelper();

            // execute dotnet restore
            processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "dotnet", "restore -binaryLogger:;ProjectImports=None -nodeReuse:false -nologo");

            string argumentsFormat = $@"msbuild {GetWarningAsErrorParameter(treatWarningsAsErrors)} -binaryLogger:;ProjectImports=None -nodeReuse:false -nologo -v:{logLevel ?? "m"} ""{_testProjectFolders.PathToSolutionFile}""";
            var msBuildProcess = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, "dotnet", argumentsFormat);

            return new CompileResult(msBuildProcess.ExitCode, msBuildProcess.CombinedOutput);
        }
    }
}
