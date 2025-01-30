using System;
using System.Collections.Generic;

namespace Reqnroll.TestProjectGenerator.Data
{
    public class Solution
    {
        private readonly List<Project> _projects = new List<Project>();

        public Solution(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IReadOnlyList<Project> Projects => _projects;

        public ProjectFile NugetConfig { get; set; }

        public List<SolutionFile> Files { get; } = new List<SolutionFile>();

        public string SdkVersion { get; set; }

        public void AddProject(Project project)
        {
            _projects.Add(project ?? throw new ArgumentNullException(nameof(project)));
        }
    }
}