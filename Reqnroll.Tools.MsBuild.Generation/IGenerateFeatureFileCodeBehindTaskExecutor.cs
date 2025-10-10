using Microsoft.Build.Framework;
using Reqnroll.CommonModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IGenerateFeatureFileCodeBehindTaskExecutor
    {
        Task<IResult<IReadOnlyCollection<ITaskItem>>> ExecuteAsync();
    }
}
