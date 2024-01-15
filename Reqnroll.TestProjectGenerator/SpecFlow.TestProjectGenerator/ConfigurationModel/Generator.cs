using System.Collections.Generic;
using TechTalk.SpecFlow.TestProjectGenerator.ConfigurationModel.Dependencies;

namespace TechTalk.SpecFlow.TestProjectGenerator.ConfigurationModel
{
    public class Generator
    {
        private readonly List<IDependency> _dependencies = new List<IDependency>();
        public bool AllowDebugGeneratedFiles { get; set; } = false;
        public bool AllowRowTests { get; set; } = true;
        public bool GenerateAsyncTests { get; set; } = false;
        public string Path { get; set; }
        public IReadOnlyList<IDependency> Dependencies => _dependencies;

        public void AddRegisterDependency(string type, string @as, string name = null)
        {
            _dependencies.Add(new RegisterDependency(type, @as, name));
        }
    }
}
