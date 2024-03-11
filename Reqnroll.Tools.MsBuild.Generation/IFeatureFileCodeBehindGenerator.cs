using System.Collections.Generic;
using Reqnroll.BoDi;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IFeatureFileCodeBehindGenerator
    {
        IEnumerable<string> GenerateFilesForProject(IReadOnlyCollection<string> featureFiles, string projectFolder, string outputPath);

    }
}
