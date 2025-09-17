using System.Collections.Generic;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IFeatureFileCodeBehindGenerator
    {
        IEnumerable<FeatureFileCodeBehindGeneratorResult> GenerateFilesForProject(IReadOnlyCollection<string> featureFiles, string projectFolder, string outputPath, string intermediateOutputPath);

    }
}
