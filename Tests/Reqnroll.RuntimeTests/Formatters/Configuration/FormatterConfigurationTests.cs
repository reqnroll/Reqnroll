using FluentAssertions;
using Reqnroll.Formatters.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class FormatterConfigurationTests
{
    [Fact]
    public void FromDictionary_Should_Return_Null_When_Dictionary_Is_Null()
    {
        // Act
        var result = FormatterConfiguration.FromDictionary(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromDictionary_Should_Extract_OutputFilePath()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>
        {
            { "outputFilePath", "test/path.txt" }
        };

        // Act
        var result = FormatterConfiguration.FromDictionary(dictionary);

        // Assert
        result.Should().NotBeNull();
        result!.OutputFilePath.Should().Be("test/path.txt");
        result.AdditionalSettings.Should().BeEmpty();
    }

    [Fact]
    public void FromDictionary_Should_Extract_OutputFilePath_Case_Insensitive()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>
        {
            { "OUTPUTFILEPATH", "test/path.txt" }
        };

        // Act
        var result = FormatterConfiguration.FromDictionary(dictionary);

        // Assert
        result.Should().NotBeNull();
        result!.OutputFilePath.Should().Be("test/path.txt");
    }

    [Fact]
    public void FromDictionary_Should_Put_Unknown_Keys_In_AdditionalSettings()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>
        {
            { "outputFilePath", "test/path.txt" },
            { "customSetting1", "value1" },
            { "customSetting2", 42 }
        };

        // Act
        var result = FormatterConfiguration.FromDictionary(dictionary);

        // Assert
        result.Should().NotBeNull();
        result!.OutputFilePath.Should().Be("test/path.txt");
        result.AdditionalSettings.Should().HaveCount(2);
        result.AdditionalSettings["customSetting1"].Should().Be("value1");
        result.AdditionalSettings["customSetting2"].Should().Be(42);
    }

    [Fact]
    public void FromDictionary_Should_Ignore_Null_Values_In_AdditionalSettings()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>
        {
            { "nullSetting", null! }
        };

        // Act
        var result = FormatterConfiguration.FromDictionary(dictionary);

        // Assert
        result.Should().NotBeNull();
        result!.AdditionalSettings.Should().BeEmpty();
    }

    [Fact]
    public void ToDictionary_Should_Include_OutputFilePath()
    {
        // Arrange
        var config = new FormatterConfiguration
        {
            OutputFilePath = "test/path.txt"
        };

        // Act
        var result = config.ToDictionary();

        // Assert
        result.Should().ContainKey("outputFilePath");
        result["outputFilePath"].Should().Be("test/path.txt");
    }

    [Fact]
    public void ToDictionary_Should_Not_Include_OutputFilePath_When_Null()
    {
        // Arrange
        var config = new FormatterConfiguration
        {
            OutputFilePath = null
        };

        // Act
        var result = config.ToDictionary();

        // Assert
        result.Should().NotContainKey("outputFilePath");
    }

    [Fact]
    public void ToDictionary_Should_Include_AdditionalSettings()
    {
        // Arrange
        var config = new FormatterConfiguration
        {
            OutputFilePath = "test/path.txt",
            AdditionalSettings = new Dictionary<string, object>
            {
                { "setting1", "value1" },
                { "setting2", 123 }
            }
        };

        // Act
        var result = config.ToDictionary();

        // Assert
        result.Should().HaveCount(3);
        result["outputFilePath"].Should().Be("test/path.txt");
        result["setting1"].Should().Be("value1");
        result["setting2"].Should().Be(123);
    }

    [Fact]
    public void RoundTrip_FromDictionary_ToDictionary_Should_Preserve_Data()
    {
        // Arrange
        var original = new Dictionary<string, object>
        {
            { "outputFilePath", "test/path.txt" },
            { "customSetting", "customValue" }
        };

        // Act
        var config = FormatterConfiguration.FromDictionary(original);
        var result = config!.ToDictionary();

        // Assert
        result["outputFilePath"].Should().Be("test/path.txt");
        result["customSetting"].Should().Be("customValue");
    }

    [Fact]
    public void GetValue_Should_Return_OutputFilePath_For_Known_Key()
    {
        // Arrange
        var config = new FormatterConfiguration
        {
            OutputFilePath = "test/path.txt"
        };

        // Act
        var result = config.GetValue<string>("outputFilePath");

        // Assert
        result.Should().Be("test/path.txt");
    }

    [Fact]
    public void GetValue_Should_Return_OutputFilePath_Case_Insensitive()
    {
        // Arrange
        var config = new FormatterConfiguration
        {
            OutputFilePath = "test/path.txt"
        };

        // Act
        var result = config.GetValue<string>("OUTPUTFILEPATH");

        // Assert
        result.Should().Be("test/path.txt");
    }

    [Fact]
    public void GetValue_Should_Return_AdditionalSetting_Value()
    {
        // Arrange
        var config = new FormatterConfiguration
        {
            AdditionalSettings = new Dictionary<string, object>
            {
                { "mySetting", "myValue" }
            }
        };

        // Act
        var result = config.GetValue<string>("mySetting");

        // Assert
        result.Should().Be("myValue");
    }

    [Fact]
    public void GetValue_Should_Return_Default_When_Key_Not_Found()
    {
        // Arrange
        var config = new FormatterConfiguration();

        // Act
        var result = config.GetValue<string>("nonExistent", "defaultValue");

        // Assert
        result.Should().Be("defaultValue");
    }

    [Fact]
    public void GetValue_Should_Convert_Numeric_Types()
    {
        // Arrange
        var config = new FormatterConfiguration
        {
            AdditionalSettings = new Dictionary<string, object>
            {
                { "intValue", 42.0 } // Stored as double from JSON
            }
        };

        // Act
        var result = config.GetValue<int>("intValue");

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public void AdditionalSettings_Should_Be_Case_Insensitive()
    {
        // Arrange
        var config = new FormatterConfiguration
        {
            AdditionalSettings = new Dictionary<string, object>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "MySetting", "myValue" }
            }
        };

        // Act & Assert
        config.AdditionalSettings["mysetting"].Should().Be("myValue");
        config.AdditionalSettings["MYSETTING"].Should().Be("myValue");
    }
}
