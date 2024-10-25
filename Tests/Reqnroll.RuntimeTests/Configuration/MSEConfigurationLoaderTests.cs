using Reqnroll.Configuration.JsonConfig;
using Reqnroll.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.IO;
using System.Reflection;

namespace Reqnroll.RuntimeTests.Configuration
{
    public class MSEConfigurationLoaderTests
    {
        private string configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Configuration\\reqnroll_config_test.json");

        [Fact]
        public void Can_Load_From_JsonFile()
        {
            var runtimeConfig = new MSE_RuntimeConfigurationLoader().LoadConfiguration(ConfigurationLoader.GetDefault(), configFilePath);
            runtimeConfig.FeatureLanguage.TwoLetterISOLanguageName.Should().Be("hu");
        }
        [Fact]
        public void Can_Load_Override_From_Environment()
        {
            //string config = @"{
            //                    ""language"": { ""feature"": ""de"" }
            //                }";
            Environment.SetEnvironmentVariable("REQNROLL__language__feature", "de");
            var runtimeConfig = new MSE_RuntimeConfigurationLoader().LoadConfiguration(ConfigurationLoader.GetDefault(), configFilePath);

            runtimeConfig.FeatureLanguage.TwoLetterISOLanguageName.Should().Be("de");
            Environment.SetEnvironmentVariable("REQNROLL__language__feature", null);
        }


    }
}
