using System.IO;
using Reqnroll.CommonModels;

namespace Reqnroll.FileAccess
{
    public interface IBinaryFileAccessor
    {
        IResult<Stream> OpenAppendOrCreateFile(string filePath);
    }
}
