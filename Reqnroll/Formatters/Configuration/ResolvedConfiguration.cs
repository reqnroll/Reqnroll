using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration
{
    public class ResolvedConfiguration
    {
        public bool Enabled { get; set; }
        public IDictionary<string, string> Formatters { get; set; }
    }
}

