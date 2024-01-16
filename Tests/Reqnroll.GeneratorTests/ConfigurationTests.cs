using FluentAssertions;
using Xunit;
using Reqnroll.Configuration;
using Reqnroll.Configuration.JsonConfig;

namespace Reqnroll.GeneratorTests
{

    public class ConfigurationTests
    {
        [Fact]
        public void CanLoadConfigWithNonParallelizableTagsProvided()
        {
            var config =
                @"{
                  ""generator"": {
                    ""addNonParallelizableMarkerForTags"": [
                      ""tag1"", ""tag2""
                    ]
                  }
                }";
            var reqnrollConfiguration = ConfigurationLoader.GetDefault();
            var configurationLoader = new JsonConfigurationLoader();
            
            reqnrollConfiguration = configurationLoader.LoadJson(reqnrollConfiguration, config);

            reqnrollConfiguration.AddNonParallelizableMarkerForTags.Should().BeEquivalentTo("tag1", "tag2");
        }
    }
}