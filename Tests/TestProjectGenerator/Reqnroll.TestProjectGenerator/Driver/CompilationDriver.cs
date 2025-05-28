using FluentAssertions;
using System;
using System.Text.RegularExpressions;

namespace Reqnroll.TestProjectGenerator.Driver
{
    public class CompilationDriver
    {
        private const BuildTool DefaultBuildTool = BuildTool.DotnetBuild;

        private readonly Compiler _compiler;
        private readonly SolutionWriteToDiskDriver _solutionWriteToDiskDriver;
        private readonly CompilationResultDriver _compilationResultDriver;
        private BuildTool? _overridenBuildTool;

        public CompilationDriver(Compiler compiler, SolutionWriteToDiskDriver solutionWriteToDiskDriver, CompilationResultDriver compilationResultDriver)
        {
            _compiler = compiler;
            _solutionWriteToDiskDriver = solutionWriteToDiskDriver;
            _compilationResultDriver = compilationResultDriver;
        }

        public bool HasTriedToCompile { get; private set; }

        public void CompileSolution(BuildTool buildTool = DefaultBuildTool, bool? treatWarningsAsErrors = null, bool failOnError = true, string logLevel = null)
        {
            CompileSolutionTimes(1, buildTool, treatWarningsAsErrors, failOnError, logLevel);
        }

        public void CompileSolutionTimes(uint times, BuildTool buildTool = DefaultBuildTool, bool? treatWarningsAsErrors = null, bool failOnError = true, string logLevel = null)
        {
            HasTriedToCompile = true;
            _solutionWriteToDiskDriver.WriteSolutionToDisk(treatWarningsAsErrors);

            var usedBuildTool = _overridenBuildTool ?? buildTool;

            for (uint time = 0; time < times; time++)
            {
                _compilationResultDriver.CompileResult = _compiler.Run(usedBuildTool, treatWarningsAsErrors, logLevel);
                if (failOnError && !_compilationResultDriver.CompileResult.IsSuccessful)
                {
                    var missingSdk = Regex.Match(_compilationResultDriver.CompileResult.Output, @"(MSB3644: .* not found\.)");
                    if (missingSdk.Success)
                        throw new DotNetSdkNotInstalledException(missingSdk.Value);
                    missingSdk = Regex.Match(_compilationResultDriver.CompileResult.Output, @"Install the .* \.NET SDK");
                    if (missingSdk.Success)
                        throw new DotNetSdkNotInstalledException(missingSdk.Value);
                    _compilationResultDriver.CompileResult.IsSuccessful.Should().BeTrue($"Compilation should succeed. Build errors: {Environment.NewLine}{_compilationResultDriver.CompileResult.ErrorLines}");
                }
            }
        }

        public void SetBuildTool(BuildTool buildTool)
        {
            _overridenBuildTool = buildTool;
        }
    }
}
