using System;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class NuGetRestorerFactory
    {
        private readonly NuGet _nuget;

        public NuGetRestorerFactory(NuGet nuget)
        {
            _nuget = nuget;
        }

        public INuGetRestorer GetNuGetRestorerForProject(Project project)
        {
            switch (project.ProjectFormat)
            {
                case ProjectFormat.New: return new SdkStyleProjectFormatNuGetRestorer();
                case ProjectFormat.Old: return new OldProjectFormatNuGetRestorer(_nuget);
                default: throw new ArgumentException($"The format of the specified project is not supported.", nameof(project));
            }
        }

    }
}
