using Reqnroll.ExternalData.ReqnrollPlugin.DataSources;

namespace Reqnroll.ExternalData.ReqnrollPlugin.Loaders
{
    public interface IDataSourceLoader
    {
        bool AcceptsSourceFilePath(string sourceFilePath);
        DataSource LoadDataSource(string path, string sourceFilePath);
    }
}
