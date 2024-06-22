using System.IO;

namespace Reqnroll.TestProjectGenerator.Cli
{
    public class SolutionConfiguration 
    { 
        public DirectoryInfo OutDir { get; internal set; }
        
        public string SolutionName { get; internal set; }
    }
}
