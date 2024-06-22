using System.Collections.Generic;
using System.Diagnostics;

namespace Reqnroll.TestProjectGenerator.Data
{
    [DebuggerDisplay("{" + nameof(Path) + ("} [{" + nameof(BuildAction) + "}]"))]
    public class ProjectFile: SolutionFile //FeatureFiles, Code, App.Config, NuGet.Config, packages.config, 
    {
        public ProjectFile(string path, string buildAction, string content, CopyToOutputDirectory copyToOutputDirectory = CopyToOutputDirectory.DoNotCopy) :
            this(path, buildAction, content, copyToOutputDirectory, new Dictionary<string, string>())
        {
        }

        public ProjectFile(string path, string buildAction, string content, CopyToOutputDirectory copyToOutputDirectory, IReadOnlyDictionary<string, string> additionalMsBuildProperties) : base(path, content)
        {
            BuildAction = buildAction;
            CopyToOutputDirectory = copyToOutputDirectory;
            AdditionalMsBuildProperties = additionalMsBuildProperties;
        }

        public string BuildAction { get; }
        public CopyToOutputDirectory CopyToOutputDirectory { get; }
        public IReadOnlyDictionary<string, string> AdditionalMsBuildProperties { get; }
    }

    public enum CopyToOutputDirectory
    {
        CopyIfNewer,
        CopyAlways,
        DoNotCopy
    }
}