﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Reqnroll.CucumberMessages.Configuration
{
    /// <summary>
    /// These classes holds configuration information from a configuration source.
    /// These are JSON serialized and correspond to the json schema in CucumberMessages-config-schema.json
    /// </summary>
    public class ConfigurationDTO : ICucumberMessagesConfiguration
    {

        public bool Enabled { get; set; }
        public string OutputFilePath { get; set; }
        public ConfigurationDTO() : this(false) { }
        public ConfigurationDTO(bool enabled)
        {
            Enabled = enabled;
        }
    }
}

