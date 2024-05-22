namespace Reqnroll.TestProjectGenerator.Data
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
