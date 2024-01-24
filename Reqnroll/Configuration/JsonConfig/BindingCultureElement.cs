using System.Runtime.Serialization;

//using Newtonsoft.Json;

namespace Reqnroll.Configuration.JsonConfig
{
    // legacy config
    public class BindingCultureElement
    {
        // legacy config
        //[JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name="name")]
        public string Name { get; set; }
    }
}