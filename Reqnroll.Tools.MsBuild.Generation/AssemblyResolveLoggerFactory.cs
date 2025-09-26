namespace Reqnroll.Tools.MsBuild.Generation
{
    public class AssemblyResolveLoggerFactory : IAssemblyResolveLoggerFactory
    {
        private readonly IReqnrollTaskLoggingHelper _log;

        public AssemblyResolveLoggerFactory(IReqnrollTaskLoggingHelper log)
        {
            _log = log;
        }

        public IAssemblyResolveLogger Build()
        {
            return new AssemblyResolveLogger(_log);
        }
    }
}
