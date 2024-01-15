namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
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

        public void CompileSolution(BuildTool buildTool = DefaultBuildTool, bool? treatWarningsAsErrors = null)
        {
            CompileSolutionTimes(1, buildTool, treatWarningsAsErrors);
        }

        public void CompileSolutionTimes(uint times, BuildTool buildTool = DefaultBuildTool, bool? treatWarningsAsErrors = null)
        {
            HasTriedToCompile = true;
            _solutionWriteToDiskDriver.WriteSolutionToDisk(treatWarningsAsErrors);

            var usedBuildTool = _overridenBuildTool ?? buildTool;

            for (uint time = 0; time < times; time++)
            {
                _compilationResultDriver.CompileResult = _compiler.Run(usedBuildTool, treatWarningsAsErrors);
            }
        }

        public void SetBuildTool(BuildTool buildTool)
        {
            _overridenBuildTool = buildTool;
        }
    }
}
