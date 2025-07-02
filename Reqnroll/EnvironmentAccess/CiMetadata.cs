using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.EnvironmentAccess
{
    public class CiMetadata
    {
        public string ProductName { get; set; }
        public string BuildUrl { get; set; }
        public string BuildNumber { get; set; }
        public string Remote { get; set; }
        public string Revision { get; set; }
        public string Branch { get; set; }
        public string Tag { get; set; }
    }
}
