namespace Reqnroll.Generator.Project
{
    public interface IReqnrollProjectReader
    {
        ReqnrollProject ReadReqnrollProject(string projectFilePath, string rootNamespace);
    }
}