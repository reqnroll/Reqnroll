using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IProjectCodeBehindGenerator
    {
        IReadOnlyCollection<ITaskItem> GenerateCodeBehindFilesForProject();
    }
}
