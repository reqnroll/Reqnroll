#if !BODI_LIMITEDRUNTIME
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Reqnroll.BoDi;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class TestConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("dependencies", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        [ConfigurationCollection(typeof(ContainerRegistrationCollection), AddItemName = "register")]
        public ContainerRegistrationCollection Dependencies
        {
            get { return (ContainerRegistrationCollection)this["dependencies"]; }
            set { this["dependencies"] = value; }
        }
    }
}
#endif