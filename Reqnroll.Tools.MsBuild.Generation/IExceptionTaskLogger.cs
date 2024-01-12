using System;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public interface IExceptionTaskLogger
    {
        void LogException(Exception exception);
    }
}
