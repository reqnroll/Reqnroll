using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Reqnroll.TestProjectGenerator.Dotnet;

public partial class SolutionCommandBuilder
{
    public class StubAddProjectSolutionCommandBuilder : AddProjectSolutionCommandBuilder
    {
        public StubAddProjectSolutionCommandBuilder(IOutputWriter outputWriter) : base(outputWriter)
        {
        }

        public override CommandBuilder Build()
        {
            return new AddProjectSolutionCommand(_outputWriter, _solutionPath, _projectPath, GetWorkingDirectory());
        }

        class AddProjectSolutionCommand : CommandBuilder
        {
            private readonly string _solutionPath;
            private readonly string _projectPath;

            public AddProjectSolutionCommand(IOutputWriter outputWriter, string solutionPath, string projectPath, string workingDirectory) 
                : base(outputWriter, "[add project to sln]", $"{projectPath} -> {solutionPath}", workingDirectory)
            {
                _solutionPath = solutionPath;
                _projectPath = projectPath;
            }

            public override CommandResult Execute(Func<Exception, Exception> exceptionFunction)
            {
                try
                {
                    var projectGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
                    var projectTypeGuid = Path.GetExtension(_projectPath).ToLowerInvariant().Equals(".vbproj") ? 
                        "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}" : 
                        "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
                    var projectName = Path.GetFileNameWithoutExtension(_projectPath);
                    var projectRelativePath = _projectPath!.Substring(Path.GetDirectoryName(_solutionPath)!.Length + 1);
                    var slnContent = File.ReadAllText(_solutionPath);
                    var projectReference =
                        $$"""
                        Project("{{projectTypeGuid}}") = "{{projectName}}", "{{projectRelativePath}}", "{{projectGuid}}"
                        EndProject
                        """;
                    var projectConfPlatforms =
                        """
                        	GlobalSection(ProjectConfigurationPlatforms) = postSolution
                        	EndGlobalSection
                        """;
                    var projectConfPlatformContent =
                        $$"""
                        		{{projectGuid}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                        		{{projectGuid}}.Debug|Any CPU.Build.0 = Debug|Any CPU
                        		{{projectGuid}}.Release|Any CPU.ActiveCfg = Release|Any CPU
                        		{{projectGuid}}.Release|Any CPU.Build.0 = Release|Any CPU
                        """;

                    slnContent = Regex.Replace(slnContent, @"\r?\nGlobal\r?\n", match => Environment.NewLine + projectReference + match.Value);
                    if (!slnContent.Contains("GlobalSection(ProjectConfigurationPlatforms)"))
                    {
                        slnContent = Regex.Replace(slnContent, @"\r?\nEndGlobal", match => Environment.NewLine + projectConfPlatforms + match.Value);
                    }

                    slnContent = Regex.Replace(slnContent, @"GlobalSection\(ProjectConfigurationPlatforms\) = postSolution\r?\n", match => match.Value + projectConfPlatformContent + Environment.NewLine);
                    File.WriteAllText(_solutionPath, slnContent);
                    _outputWriter.WriteLine($"Solution file updated: {ArgumentsFormat}");
                    return new CommandResult(0, "Solution file updated.");
                }
                catch (Exception ex)
                {
                    throw exceptionFunction(ex);
                }
            }
        }
    }
}
