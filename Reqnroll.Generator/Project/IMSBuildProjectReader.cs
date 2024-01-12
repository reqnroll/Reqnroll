namespace Reqnroll.Generator.Project
{
    public interface IMSBuildProjectReader
    {
        ReqnrollProject LoadReqnrollProjectFromMsBuild(string projectFilePath, string rootNamespace);
    }
}
