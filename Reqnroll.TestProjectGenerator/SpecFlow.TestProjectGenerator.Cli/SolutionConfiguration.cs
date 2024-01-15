using System.IO;

namespace SpecFlow.TestProjectGenerator.Cli
{
    public class SolutionConfiguration 
    { 
        public DirectoryInfo OutDir { get; internal set; }
        
        public string SolutionName { get; internal set; }
    }
}
