namespace Reqnroll.ExternalData.ReqnrollPlugin.Loaders
{
    public interface IDataSourceLoaderFactory
    {
        IDataSourceLoader CreateLoader(string format, string dataSourcePath);
    }
}
