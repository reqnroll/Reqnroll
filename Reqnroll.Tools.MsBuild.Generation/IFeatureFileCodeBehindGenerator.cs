using System.Collections.Generic;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IFeatureFileCodeBehindGenerator
    {
        IEnumerable<string> GenerateFilesForProject(IReadOnlyCollection<string> featureFiles, string projectFolder, string outputPath, bool featureFilesEmbedded);

    }
}
