namespace Reqnroll.TestProjectGenerator.Data
{
    public class ReqnrollPlugin
    {
        public ReqnrollPlugin(string name)
        {
            Name = name;
        }

        public ReqnrollPlugin(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public ReqnrollPlugin(string name, ReqnrollPluginType type)
        {
            Name = name;
            Type = type;
        }

        public ReqnrollPlugin(string name, string path, ReqnrollPluginType type)
        {
            Name = name;
            Path = path;
            Type = type;
        }

        public string Name { get; }
        public string Path { get; }
        public ReqnrollPluginType Type { get; } = ReqnrollPluginType.Generator | ReqnrollPluginType.Runtime;
    }
}