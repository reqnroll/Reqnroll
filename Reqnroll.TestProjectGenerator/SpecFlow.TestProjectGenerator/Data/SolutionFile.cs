namespace TechTalk.SpecFlow.TestProjectGenerator.Data
{
    public class SolutionFile
    {
        public SolutionFile(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public string Path { get; } //relative from project
        public string Content { get; }
    }
}