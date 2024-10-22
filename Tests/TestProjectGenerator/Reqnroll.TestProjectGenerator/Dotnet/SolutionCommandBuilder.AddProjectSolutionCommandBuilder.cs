using System;
using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet
{
    public partial class SolutionCommandBuilder
    {
        public class AddProjectSolutionCommandBuilder : BaseCommandBuilder
        {
            protected string _solutionPath;
            protected string _projectPath;

         
            public AddProjectSolutionCommandBuilder ToSolution(string solutionPath)
            {
                _solutionPath = solutionPath;
                return this;
            }

            public AddProjectSolutionCommandBuilder Project(string projectPath)
            {
                _projectPath = projectPath;
                return this;
            }

            protected override string GetWorkingDirectory()
            {
                return Path.GetDirectoryName(_solutionPath);
            }

            protected override string BuildArguments()
            {
                if (string.IsNullOrWhiteSpace(_solutionPath)) throw new ArgumentNullException("Solution is not set");
                if (string.IsNullOrWhiteSpace(_projectPath)) throw new ArgumentNullException("Project is not set");

                var arguments = $"sln \"{_solutionPath}\" add \"{_projectPath}\"";

                return arguments;
            }

            public AddProjectSolutionCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
            {
            }
        }
    }
}
