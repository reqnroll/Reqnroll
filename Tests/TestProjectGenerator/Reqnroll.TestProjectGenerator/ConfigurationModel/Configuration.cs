using System;
using System.Collections.Generic;
using System.Globalization;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.ConfigurationModel
{
    public class Configuration
    {
        public UnitTestProvider UnitTestProvider { get; set; } = UnitTestProvider.xUnit;
        public List<ReqnrollPlugin> Plugins { get; } = new List<ReqnrollPlugin>();
        public List<AppConfigSection> AppConfigSection { get;  } = new List<AppConfigSection> { new AppConfigSection(name: "reqnroll", type: "Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.ConfigurationSectionHandler, Reqnroll.SpecFlowCompatibility.ReqnrollPlugin") };
        public List<BindingAssembly> BindingAssemblies { get;  } = new List<BindingAssembly>();
        public CultureInfo FeatureLanguage { get; set; } = CultureInfo.GetCultureInfo("en-US");
        public CultureInfo BindingCulture { get; set; }
        public Lazy<Generator> Generator { get; } = new Lazy<Generator>(() => new Generator());
        public Lazy<Runtime> Runtime { get; } = new Lazy<Runtime>(() => new Runtime());
        public List<(string key, string value)> AppSettings { get; set; } = new List<(string key, string value)>();
    }
}
