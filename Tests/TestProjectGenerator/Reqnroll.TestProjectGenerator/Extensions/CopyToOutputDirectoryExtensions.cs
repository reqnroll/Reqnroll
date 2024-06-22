using System;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.Extensions
{
    public static class CopyToOutputDirectoryExtensions
    {
        public static string GetCopyToOutputDirectoryString(this CopyToOutputDirectory fileCopyToOutputDirectory)
        {
            switch (fileCopyToOutputDirectory)
            {
                case CopyToOutputDirectory.CopyIfNewer:
                    return "PreserveNewest";
                case CopyToOutputDirectory.CopyAlways:
                    return "Always";
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileCopyToOutputDirectory), fileCopyToOutputDirectory, null);
            }
        }
    }
}