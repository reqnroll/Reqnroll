using Reqnroll.TestProjectGenerator;

namespace Reqnroll.Specs.Support
{
    class OutputConnector : IOutputWriter
    {
        private readonly IReqnrollOutputHelper _reqnrollOutputHelper;

        public OutputConnector(IReqnrollOutputHelper reqnrollOutputHelper)
        {
            _reqnrollOutputHelper = reqnrollOutputHelper;
        }

        public void WriteLine(string message)
        {
            _reqnrollOutputHelper.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            _reqnrollOutputHelper.WriteLine(format, args);
        }
    }
}
