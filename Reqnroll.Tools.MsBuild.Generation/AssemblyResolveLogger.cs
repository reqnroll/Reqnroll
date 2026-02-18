using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Tools.MsBuild.Generation;

public sealed class AssemblyResolveLogger : IAssemblyResolveLogger
{
    private readonly IReqnrollTaskLoggingHelper _log;
    private bool _isDisposed;
    private readonly string _taskFolder;

    public AssemblyResolveLogger(IReqnrollTaskLoggingHelper log)
    {
        _log = log;
        _taskFolder = Path.GetDirectoryName(typeof(AssemblyResolveLogger).Assembly.Location);
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    ~AssemblyResolveLogger()
    {
        Dispose(false);
    }

    public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        _log.LogTaskDiagnosticMessage($"Resolving {args.Name}");

        try
        {
            var requestedAssemblyName = new AssemblyName(args.Name);
                
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == requestedAssemblyName.Name);
            if (loadedAssembly != null)
            {
                _log.LogTaskDiagnosticMessage($"  Loading {args.Name} from loaded assembly ('{loadedAssembly.FullName}')");
                return loadedAssembly;
            }

            if (_taskFolder != null)
            {
                var assemblyPath = Path.Combine(_taskFolder, requestedAssemblyName.Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    _log.LogTaskDiagnosticMessage($"  Loading {args.Name} from {assemblyPath}");
                    return Assembly.LoadFrom(assemblyPath);
                }
            }

            _log.LogTaskDiagnosticMessage($"  {args.Name} is not in folder {_taskFolder}");
        }
        catch (Exception ex)
        {
            _log.LogTaskError(ex.Message);
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