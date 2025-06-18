using Reqnroll.Formatters.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reqnroll.GeneratorTests
{
    internal class SimpleCucumberMessagesConfigurationProvider : IFormattersConfigurationProvider
    {
        public bool Enabled => false;

        public string GetFormatterConfigurationByName(string formatterName)
        {
            return "\"outputFilePath\" : \"reqnroll_report.ndjson\"";
        }
    }
}
