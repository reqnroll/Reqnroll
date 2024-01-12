using System.Collections.Generic;
using Microsoft.Build.Framework;
using Reqnroll.CommonModels;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IGenerateFeatureFileCodeBehindTaskExecutor
    {
        IResult<IReadOnlyCollection<ITaskItem>> Execute();
    }
}
