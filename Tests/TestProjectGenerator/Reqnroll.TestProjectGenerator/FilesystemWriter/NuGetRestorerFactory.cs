using System;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
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
                // the old (non-SDK-style) projects also use PackageReference, so they are restored as part of the build
                case ProjectFormat.Old: return new SdkStyleProjectFormatNuGetRestorer();
                default: throw new ArgumentException($"The format of the specified project is not supported.", nameof(project));
            }
        }

    }
}
