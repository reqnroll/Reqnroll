using FluentAssertions;
using Reqnroll.Configuration.JsonConfig;
using Reqnroll.Formatters.Configuration;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class FormattersConfigExtractorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractFormatters_Should_Return_Empty_For_NullOrWhitespace(string input)
    {
        FormattersConfigExtractor.ExtractFormatters(input).Should().BeEmpty();
    }

    [Fact]
    public void ExtractFormatters_Should_Return_Empty_For_Invalid_Json()
    {
        FormattersConfigExtractor.ExtractFormatters("{ not json }").Should().BeEmpty();
    }

    [Fact]
    public void ExtractFormatters_Should_Return_Empty_When_No_Formatters_Key()
    {
        FormattersConfigExtractor.ExtractFormatters(@"{ ""other"": {} }").Should().BeEmpty();
    }

    [Fact]
    public void ExtractFormatters_Should_Return_Formatter_With_OutputFilePath()
    {
        var result = FormattersConfigExtractor.ExtractFormatters(@"{
            ""formatters"": { ""html"": { ""outputFilePath"": ""out.html"" } }
        }");

        result["html"].OutputFilePath.Should().Be("out.html");
    }

    [Fact]
    public void ConvertFormatterOptions_Should_Return_Empty_Config_For_Null()
    {
        var result = FormattersConfigExtractor.ConvertFormatterOptions(null);

        result.Should().NotBeNull();
        result.OutputFilePath.Should().BeNull();
        result.AdditionalSettings.Should().BeEmpty();
    }

    [Theory]
    [InlineData("stringVal", "hello", "hello")]
    public void ConvertFormatterOptions_Should_Map_String_AdditionalOption(string key, string jsonValue, string expected)
    {
        var options = new FormatterOptionsElement
        {
            AdditionalOptions = new Dictionary<string, JsonElement>
            {
                { key, JsonDocument.Parse($@"""{jsonValue}""").RootElement }
            }
        };

        var result = FormattersConfigExtractor.ConvertFormatterOptions(options);

        result.AdditionalSettings[key].Should().Be(expected);
    }

    [Fact]
    public void ConvertFormatterOptions_Should_Map_Boolean_AdditionalOption()
    {
        var options = new FormatterOptionsElement
        {
            AdditionalOptions = new Dictionary<string, JsonElement>
            {
                { "flag", JsonDocument.Parse("true").RootElement }
            }
        };

        FormattersConfigExtractor.ConvertFormatterOptions(options)
            .AdditionalSettings["flag"].Should().Be(true);
    }

    [Fact]
    public void ConvertFormatterOptions_Should_Map_Integer_AdditionalOption()
    {
        var options = new FormatterOptionsElement
        {
            AdditionalOptions = new Dictionary<string, JsonElement>
            {
                { "count", JsonDocument.Parse("42").RootElement }
            }
        };

        FormattersConfigExtractor.ConvertFormatterOptions(options)
            .AdditionalSettings["count"].Should().Be(42L);
    }

    [Fact]
    public void ConvertFormatterOptions_Should_Exclude_Null_AdditionalOption()
    {
        var options = new FormatterOptionsElement
        {
            AdditionalOptions = new Dictionary<string, JsonElement>
            {
                { "nullKey", JsonDocument.Parse("null").RootElement }
            }
        };

        FormattersConfigExtractor.ConvertFormatterOptions(options)
            .AdditionalSettings.Should().NotContainKey("nullKey");
    }
}
