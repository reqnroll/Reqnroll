using FluentAssertions;
using Xunit;
using Reqnroll.Configuration;
using Reqnroll.Configuration.AppConfig;

namespace Reqnroll.GeneratorTests
{

    public class ConfigurationTests
    {
        [Fact]
        public void CanLoadConfigWithNonParallelizableTagsProvided()
        {
            var config =
                @"<reqnroll>
                    <generator>
                        <addNonParallelizableMarkerForTags>
                            <tag value=""tag1""/>
                            <tag value=""tag2""/>
                        </addNonParallelizableMarkerForTags>
                    </generator>
                </reqnroll>";
            var reqnrollConfiguration = ConfigurationLoader.GetDefault();
            var configurationLoader = new AppConfigConfigurationLoader();
            
            var configurationSectionHandler = ConfigurationSectionHandler.CreateFromXml(config);
            reqnrollConfiguration = configurationLoader.LoadAppConfig(reqnrollConfiguration, configurationSectionHandler);

            reqnrollConfiguration.AddNonParallelizableMarkerForTags.Should().BeEquivalentTo("tag1", "tag2");
        }
    }
}