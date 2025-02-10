using Reqnroll.CucumberMessages.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.GeneratorTests
{
    internal class SimpleCucumberMessagesConfiguration : ICucumberMessagesConfiguration
    {
        public bool Enabled => false;

        public string OutputFilePath => "reqnroll_report.ndjson";

        public IDGenerationStyle IDGenerationStyle => IDGenerationStyle.Incrementing;
    }
}
