using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.Configuration
{
    public class CucumberMessagesConfiguration
    {
        public bool Enabled { get; set; }
        public string BaseDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public string OutputFileName { get; set; }
        public IDGenerationStyle IDGenerationStyle { get; set; }

    }
}
