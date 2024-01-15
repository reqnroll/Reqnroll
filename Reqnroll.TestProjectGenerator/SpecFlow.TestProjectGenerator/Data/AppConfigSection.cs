namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class AppConfigSection
    {
        public AppConfigSection(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public string Type { get; }
    }
}
