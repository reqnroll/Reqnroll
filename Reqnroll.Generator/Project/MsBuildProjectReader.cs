namespace Reqnroll.Generator.Project
{
    public class MSBuildProjectReader : IMSBuildProjectReader
    {
        private readonly IReqnrollProjectReader _projectReader;

        public MSBuildProjectReader(IReqnrollProjectReader projectReader)
        {
            _projectReader = projectReader;
        }

        public ReqnrollProject LoadReqnrollProjectFromMsBuild(string projectFilePath, string rootNamespace)
        {
            return _projectReader.ReadReqnrollProject(projectFilePath, rootNamespace);
        }
    }
}
