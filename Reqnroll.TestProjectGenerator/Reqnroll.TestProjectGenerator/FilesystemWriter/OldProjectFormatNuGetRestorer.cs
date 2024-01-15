using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public class OldProjectFormatNuGetRestorer : INuGetRestorer
    {
        private readonly NuGet _nuget;

        public OldProjectFormatNuGetRestorer(NuGet nuget)
        {
            _nuget = nuget;
        }

        public void RestoreForProject(Project project)
        {
            _nuget.RestoreForProject(project);
        }
    }
}