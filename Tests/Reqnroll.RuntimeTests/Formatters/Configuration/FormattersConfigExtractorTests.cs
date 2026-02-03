using FluentAssertions;
using Reqnroll.Configuration.JsonConfig;
using Reqnroll.Formatters.Configuration;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class FormattersConfigExtractorTests
{
    [Fact]
    public void ExtractFormatters_Should_Return_Empty_Dictionary_When_Json_Is_Null()
    {
        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractFormatters_Should_Return_Empty_Dictionary_When_Json_Is_Empty()
    {
        // Act
        var result = FormattersConfigExtractor.ExtractFormatters("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractFormatters_Should_Return_Empty_Dictionary_When_Json_Has_No_Formatters()
    {
        // Arrange
        var json = @"{ ""language"": { ""feature"": ""en"" } }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractFormatters_Should_Return_Empty_Dictionary_For_Invalid_Json()
    {
        // Arrange
        var invalidJson = "{ not valid json }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(invalidJson);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractFormatters_Should_Extract_Known_Html_Formatter()
    {
        // Arrange
        var json = @"{
            ""formatters"": {
                ""html"": { ""outputFilePath"": ""report.html"" }
            }
        }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        result.Should().ContainKey("html");
        result["html"]["outputFilePath"].Should().Be("report.html");
    }

    [Fact]
    public void ExtractFormatters_Should_Extract_Known_Message_Formatter()
    {
        // Arrange
        var json = @"{
            ""formatters"": {
                ""message"": { ""outputFilePath"": ""messages.ndjson"" }
            }
        }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        result.Should().ContainKey("message");
        result["message"]["outputFilePath"].Should().Be("messages.ndjson");
    }

    [Fact]
    public void ExtractFormatters_Should_Extract_Custom_Formatter_Via_AdditionalFormatters()
    {
        // Arrange
        var json = @"{
            ""formatters"": {
                ""customFormatter"": { ""setting1"": ""value1"", ""setting2"": ""value2"" }
            }
        }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        result.Should().ContainKey("customFormatter");
        result["customFormatter"]["setting1"].Should().Be("value1");
        result["customFormatter"]["setting2"].Should().Be("value2");
    }

    [Fact]
    public void ExtractFormatters_Should_Extract_Multiple_Formatters()
    {
        // Arrange
        var json = @"{
            ""formatters"": {
                ""html"": { ""outputFilePath"": ""report.html"" },
                ""message"": { ""outputFilePath"": ""messages.ndjson"" },
                ""custom"": { ""customSetting"": ""customValue"" }
            }
        }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainKeys("html", "message", "custom");
    }

    [Fact]
    public void ExtractFormatters_Should_Handle_Additional_Options_On_Known_Formatter()
    {
        // Arrange
        var json = @"{
            ""formatters"": {
                ""html"": { 
                    ""outputFilePath"": ""report.html"",
                    ""customOption"": ""customValue""
                }
            }
        }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        result["html"]["outputFilePath"].Should().Be("report.html");
        result["html"]["customOption"].Should().Be("customValue");
    }

    [Fact]
    public void ExtractFormatters_Should_Handle_Boolean_Config_Values()
    {
        // Arrange
        var json = @"{
            ""formatters"": {
                ""custom"": { ""enabled"": true, ""debug"": false }
            }
        }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        result["custom"]["enabled"].Should().Be(true);
        result["custom"]["debug"].Should().Be(false);
    }

    [Fact]
    public void ExtractFormatters_Should_Handle_Numeric_Config_Values()
    {
        // Arrange
        var json = @"{
            ""formatters"": {
                ""custom"": { ""timeout"": 30.5, ""retries"": 3 }
            }
        }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        result["custom"]["timeout"].Should().Be(30.5);
        result["custom"]["retries"].Should().Be(3.0); // Numbers are parsed as double
    }

    [Fact]
    public void ConvertFormattersElement_Should_Return_Empty_Dictionary_When_Null()
    {
        // Act
        var result = FormattersConfigExtractor.ConvertFormattersElement(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ConvertFormattersElement_Should_Return_Empty_Dictionary_When_No_Formatters_Defined()
    {
        // Arrange
        var element = new FormattersElement();

        // Act
        var result = FormattersConfigExtractor.ConvertFormattersElement(element);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ConvertFormattersElement_Should_Handle_Empty_Formatter_Options()
    {
        // Arrange
        var element = new FormattersElement
        {
            Html = new FormatterOptionsElement()
        };

        // Act
        var result = FormattersConfigExtractor.ConvertFormattersElement(element);

        // Assert
        result.Should().ContainKey("html");
        result["html"].Should().BeEmpty();
    }

    [Fact]
    public void ExtractFormatters_Should_Be_Case_Insensitive_For_Formatter_Names()
    {
        // Arrange
        var json = @"{
            ""formatters"": {
                ""HTML"": { ""outputFilePath"": ""report.html"" }
            }
        }";

        // Act
        var result = FormattersConfigExtractor.ExtractFormatters(json);

        // Assert
        // The key should be accessible case-insensitively due to StringComparer.OrdinalIgnoreCase
        result["html"]["outputFilePath"].Should().Be("report.html");
        result["HTML"]["outputFilePath"].Should().Be("report.html");
    }
}
