namespace Reqnroll.TestProjectGenerator.Data
{
    public class MSBuildTarget
    {
        public MSBuildTarget(string name, string implementation)
        {
            Name = name;
            Implementation = implementation;
        }

        public string Name { get; private set; }
        public string Implementation { get; private set; }
    }
}