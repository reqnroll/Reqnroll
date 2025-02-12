using System.Collections.Generic;
using Reqnroll.TestProjectGenerator.ConfigurationModel.Dependencies;

namespace Reqnroll.TestProjectGenerator.ConfigurationModel
{
    public class Generator
    {
        private readonly List<IDependency> _dependencies = new List<IDependency>();
        private readonly List<string> _addNonParallelizableMarkerForTags = new List<string>();
        public bool AllowDebugGeneratedFiles { get; set; } = false;
        public bool AllowRowTests { get; set; } = true;
        public bool GenerateAsyncTests { get; set; } = false;
        public string Path { get; set; }
        public IReadOnlyList<IDependency> Dependencies => _dependencies;
        public IReadOnlyList<string> AddNonParallelizableMarkerForTags => _addNonParallelizableMarkerForTags;

        public void AddRegisterDependency(string type, string @as, string name = null)
        {
            _dependencies.Add(new RegisterDependency(type, @as, name));
        }

        public void AddNonParallelizableMarkerForTag(string tagName)
        {
            _addNonParallelizableMarkerForTags.Add(tagName);
        }
    }
}
