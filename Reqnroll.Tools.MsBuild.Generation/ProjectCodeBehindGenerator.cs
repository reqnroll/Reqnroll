using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class ProjectCodeBehindGenerator : IProjectCodeBehindGenerator
    {
        public static string EmbeddedMessagesStoragePath = "EmbeddedMessagesStoragePath";
        public static string EmbeddedMessagesResourceName = "EmbeddedMessagesResourceName";

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
                _reqnrollProjectInfo.IntermediateOutputPath);

            return generatedFiles.Select(item =>
                                {
                                    var result = new TaskItem { ItemSpec = item.CodeBehindRelativePath };
                                    result.SetMetadata(EmbeddedMessagesStoragePath, item.EmbeddedMessagesStoragePath);
                                    result.SetMetadata(EmbeddedMessagesResourceName, item.EmbeddedMessagesResourceName);
                                    return result;
                                })
                                .Cast<ITaskItem>()
                                .ToArray();
        }
    }
}
