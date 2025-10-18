using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Generator.Plugins
{
    public class GeneratorPluginParameters
    {
        public string Parameters { get; set; }

        public IDictionary<string, string> GetParametersAsDictionary()
        {
            if (string.IsNullOrWhiteSpace(Parameters))
                return new Dictionary<string, string>();

            return Parameters
                   .Split([';'], StringSplitOptions.RemoveEmptyEntries)
                   .Select(setting => setting.Split(['='], 2))
                   .Where(keyValue => keyValue.Length == 2)
                   .ToDictionary(keyValue => keyValue[0], keyValue => keyValue[1]);
        }
    }
}