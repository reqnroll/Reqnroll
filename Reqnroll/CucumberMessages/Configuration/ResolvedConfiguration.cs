using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.Configuration
{
    public class ResolvedConfiguration
    {
        public bool Enabled { get; set; }
        public IDictionary<string, string> Formatters { get; set; }
    }
}

