using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class ProjectCodeBehindGenerator : IProjectCodeBehindGenerator
    {
        private readonly IFeatureFileCodeBehindGenerator _featureFileCodeBehindGenerator;
        private readonly ReqnrollProjectInfo _reqnrollProjectInfo;

        public ProjectCodeBehindGenerator(IFeatureFileCodeBehindGenerator featureFileCodeBehindGenerator, ReqnrollProjectInfo reqnrollProjectInfo)
        {
            _featureFileCodeBehindGenerator = featureFileCodeBehindGenerator;
            _reqnrollProjectInfo = reqnrollProjectInfo;
        }

        public IReadOnlyCollection<ITaskItem> GenerateCodeBehindFilesForProject()
        {
            var generatedFiles = _featureFileCodeBehindGenerator.GenerateFilesForProject(
                _reqnrollProjectInfo.FeatureFiles,
                _reqnrollProjectInfo.ProjectFolder,
                _reqnrollProjectInfo.OutputPath,
                _reqnrollProjectInfo.FeatureFilesEmbedded);

            return generatedFiles.Select(file => new TaskItem { ItemSpec = file })
                                 .Cast<ITaskItem>()
                                 .ToArray();
        }
    }
}
