namespace TechTalk.SpecFlow.TestProjectGenerator.Data
{
    public class MSBuildImport
    {
        public string MsbuildTargetFile { get; }

        public MSBuildImport(string msbuildTargetFile)
        {
            MsbuildTargetFile = msbuildTargetFile;
        }
    }
}
