using System.Runtime.Serialization;

namespace Reqnroll.Configuration.JsonConfig
{
    public class Dependency
    {
        [DataMember(Name = "type")]
        public string ImplementationType { get; set; }
        [DataMember(Name = "as")]
        public string InterfaceType { get; set; }
    }
}