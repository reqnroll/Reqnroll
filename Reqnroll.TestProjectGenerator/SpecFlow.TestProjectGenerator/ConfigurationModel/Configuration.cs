using System;
using System.Collections.Generic;
using System.Globalization;
using TechTalk.SpecFlow.TestProjectGenerator.Data;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace TechTalk.SpecFlow.TestProjectGenerator.ConfigurationModel
{
    public class Configuration
    {
        public UnitTestProvider UnitTestProvider { get; set; } = UnitTestProvider.xUnit;
        public List<SpecFlowPlugin> Plugins { get; } = new List<SpecFlowPlugin>();
        public List<AppConfigSection> AppConfigSection { get;  } = new List<AppConfigSection> { new AppConfigSection(name: "specFlow", type: "TechTalk.SpecFlow.Configuration.ConfigurationSectionHandler, TechTalk.SpecFlow") };
        public List<StepAssembly> StepAssemblies { get;  } = new List<StepAssembly>();
        public CultureInfo FeatureLanguage { get; set; } = CultureInfo.GetCultureInfo("en-US");
        public CultureInfo BindingCulture { get; set; }
        public Lazy<Generator> Generator { get; } = new Lazy<Generator>(() => new Generator());
        public Lazy<Runtime> Runtime { get; } = new Lazy<Runtime>(() => new Runtime());
        public List<(string key, string value)> AppSettings { get; set; } = new List<(string key, string value)>();
    }
}
