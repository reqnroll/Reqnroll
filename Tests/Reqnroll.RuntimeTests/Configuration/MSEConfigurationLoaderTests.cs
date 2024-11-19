using Reqnroll.Configuration.JsonConfig;
using Reqnroll.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using System.IO;
using System.Reflection;

namespace Reqnroll.RuntimeTests.Configuration
{
    public class MSEConfigurationLoaderTests
    {
        private IReqnrollJsonLocator GetStubJsonLocator()
        {
            var stubJsonLocatorMock = new Mock<IReqnrollJsonLocator>();
            stubJsonLocatorMock.Setup(x => x.GetReqnrollJsonFilePath()).Returns(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Configuration\\reqnroll_config_test.json"));
            return stubJsonLocatorMock.Object;
        }
        //private string configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Configuration\\reqnroll_config_test.json");

        [Fact]
        public void Can_Load_From_JsonFile()
        {
            var runtimeConfig = new MicrosoftConfiguration_RuntimeConfigurationLoader(GetStubJsonLocator(), new Microsoft.Extensions.Configuration.ConfigurationManager())
                .Load(ConfigurationLoader.GetDefault());
            runtimeConfig.FeatureLanguage.TwoLetterISOLanguageName.Should().Be("hu");
        }
        [Fact]
        public void Can_Load_Override_From_Environment()
        {
            //string config = @"{
            //                    ""language"": { ""feature"": ""de"" }
            //                }";
            Environment.SetEnvironmentVariable("REQNROLL__language__feature", "de");
            var runtimeConfig = new MicrosoftConfiguration_RuntimeConfigurationLoader(GetStubJsonLocator(), new Microsoft.Extensions.Configuration.ConfigurationManager())
                .Load(ConfigurationLoader.GetDefault());

            runtimeConfig.FeatureLanguage.TwoLetterISOLanguageName.Should().Be("de");
            Environment.SetEnvironmentVariable("REQNROLL__language__feature", null);
        }


    }
}
