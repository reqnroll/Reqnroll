using System;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public sealed class AssemblyResolveLogger : IAssemblyResolveLogger
    {
        private readonly ITaskLoggingWrapper _taskLoggingWrapper;
        private bool _isDisposed;

        public AssemblyResolveLogger(ITaskLoggingWrapper taskLoggingWrapper)
        {
            _taskLoggingWrapper = taskLoggingWrapper;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        ~AssemblyResolveLogger()
        {
            Dispose(false);
        }

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            _taskLoggingWrapper.LogMessage(args.Name);

            try
            {
                var requestedAssemblyName = new AssemblyName(args.Name);
                var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == requestedAssemblyName.Name);
                if (loadedAssembly != null) return loadedAssembly;
            }
            catch (Exception ex)
            {
                _taskLoggingWrapper.LogError(ex.Message);
                return null;
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }

            _isDisposed = true;
        }
    }
}
