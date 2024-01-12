namespace Reqnroll.Plugins
{
    public class ReqnrollPath : IReqnrollPath
    {
        public string GetPathToReqnrollDll()
        {
            return typeof(ReqnrollPath).Assembly.Location;
        }
    }
}
