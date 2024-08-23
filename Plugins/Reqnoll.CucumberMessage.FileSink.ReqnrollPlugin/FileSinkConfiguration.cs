using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    internal class FileSinkConfiguration
    {
        public bool FileSinkEnabled { get; set; }
        public List<Destination> Destinations { get; set; }

        public FileSinkConfiguration() : this(true) { }
        public FileSinkConfiguration(bool fileSinkEnabled) : this(fileSinkEnabled, new List<Destination>()) { }
         public FileSinkConfiguration(bool fileSinkEnabled, List<Destination> destinations)
        {
            FileSinkEnabled = fileSinkEnabled;
            Destinations = destinations;
        }
    }

    public class Destination
    {
        public bool Enabled { get; set; }
        public string BasePath { get; set; }
        public string OutputDirectory { get; set; }

        public Destination(bool enabled, string basePath, string outputDirectory)
        {
            Enabled = true;
            BasePath = basePath;
            OutputDirectory = outputDirectory;
        }
    }
}

