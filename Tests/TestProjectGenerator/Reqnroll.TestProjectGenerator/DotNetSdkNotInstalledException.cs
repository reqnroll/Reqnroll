using System;

namespace Reqnroll.TestProjectGenerator
{
    /// <summary>
    /// Is thrown when a needed SDK/TargetFramework is not installed.
    /// </summary>
    /// <remarks>
    /// Is used to ignore tests, if the needed SDKs/TargetFrameworks are not installed on the local machine.
    /// This helps to improve the (first time) developer experience.
    /// </remarks>
    public class DotNetSdkNotInstalledException : ProjectCreationNotPossibleException
    {
        public DotNetSdkNotInstalledException(string message) : base(message)
        {
        }

        public DotNetSdkNotInstalledException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
