namespace TechTalk.SpecFlow.TestProjectGenerator.Data
{
    public class SpecFlowPlugin
    {
        public SpecFlowPlugin(string name)
        {
            Name = name;
        }

        public SpecFlowPlugin(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public SpecFlowPlugin(string name, SpecFlowPluginType type)
        {
            Name = name;
            Type = type;
        }

        public SpecFlowPlugin(string name, string path, SpecFlowPluginType type)
        {
            Name = name;
            Path = path;
            Type = type;
        }

        public string Name { get; }
        public string Path { get; }
        public SpecFlowPluginType Type { get; } = SpecFlowPluginType.Generator | SpecFlowPluginType.Runtime;
    }
}