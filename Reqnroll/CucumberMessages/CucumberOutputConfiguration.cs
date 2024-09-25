using Reqnroll.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class CucumberOutputConfiguration
    {

        public bool FileOutputEnabled { get; set; }
        public List<Destination> Destinations { get; set; }

        public CucumberOutputConfiguration() : this(true) { }
        public CucumberOutputConfiguration(bool fileSinkEnabled) : this(fileSinkEnabled, new List<Destination>()) { }
        public CucumberOutputConfiguration(bool fileSinkEnabled, List<Destination> destinations)
        {
            FileOutputEnabled = fileSinkEnabled;
            Destinations = destinations;
        }

    }

    public class Destination
    {
        public string ProfileName { get; set; }
        public string BasePath { get; set; }
        public string OutputDirectory { get; set; }

        public Destination(string profileName, string basePath, string outputDirectory)
        {
            ProfileName = String.IsNullOrEmpty(profileName) ? "DEFAULT" : profileName;
            BasePath = basePath;
            OutputDirectory = outputDirectory;
        }
    }
}

