using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Reqnroll.Formatters.Configuration;
using Reqnroll.MessagesLogger.TestLogger;
using Xunit;
using System.Reflection;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class TestVSTestIntegrationConfigurationResolver : VsTestIntegrationConfigurationResolver
{
    public TestVSTestIntegrationConfigurationResolver() : base()
    {
    }
    public new JsonDocument GetJsonDocument() => base.GetJsonDocument();
}
public class VSTestIntegrationConfigurationResolverTests
{
    public VSTestIntegrationConfigurationResolverTests()
    {
        // Reset FormattersLogger static state before each test
        typeof(FormattersLogger)
            .GetProperty("IsInitialized", BindingFlags.Static | BindingFlags.Public)
            ?.SetValue(null, false);
        typeof(FormattersLogger)
            .GetProperty("HasParameters", BindingFlags.Static | BindingFlags.Public)
            ?.SetValue(null, false);
        typeof(FormattersLogger)
            .GetProperty("Parameters", BindingFlags.Static | BindingFlags.Public)
            ?.SetValue(null, new Dictionary<string, string>());
    }

    [Fact]
    public void GetJsonDocument_ShouldReturnEmptyObject_WhenLoggerNotInitialized()
    {
        // Arrange
        var resolver = CreateResolver();

        // Act
        var doc = resolver.GetJsonDocument();

        // Assert
        doc.RootElement.ToString().Should().Be("{}");
    }

    [Fact]
    public void GetJsonDocument_ShouldReturnEmptyObject_WhenLoggerHasNoParameters()
    {
        // Arrange
        typeof(FormattersLogger).GetProperty("IsInitialized", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
        typeof(FormattersLogger).GetProperty("HasParameters", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, false);
        var resolver = CreateResolver();

        // Act
        var doc = resolver.GetJsonDocument();

        // Assert
        doc.RootElement.ToString().Should().Be("{}");
    }

    [Fact]
    public void GetJsonDocument_ShouldReturnParametersAsJson_WhenLoggerHasParameters()
    {
        // Arrange
        typeof(FormattersLogger).GetProperty("IsInitialized", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
        typeof(FormattersLogger).GetProperty("HasParameters", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
        typeof(FormattersLogger).GetProperty("Parameters", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, new Dictionary<string, string>
        {
            { "html", @"outputFileName:c\:/html/report.html" },
            { "json", @"outputFileName:c\:/json/report.json" }
        });
        var resolver = CreateResolver();

        // Act
        var doc = resolver.GetJsonDocument();

        // Assert
        var root = doc.RootElement;
        root.TryGetProperty("html", out var htmlProp).Should().BeTrue();
        htmlProp.ToString().Should().Be("{\"outputFileName\": \"c:/html/report.html\"}");
        root.TryGetProperty("json", out var jsonProp).Should().BeTrue();
        jsonProp.ToString().Should().Be("{\"outputFileName\": \"c:/json/report.json\"}");
    }

    [Fact]
    public void ReconstructJsonValue_ShouldHandleMultiPropertyTopLevelObjects()
    {
        // Arrange
        var resolver = CreateResolver();
        // Act
        var json = (string)resolver.ReconstructJsonValue("key1:value1,key2:value2");

        // Assert
        json.Should().Be("{\"key1\": \"value1\",\"key2\": \"value2\"}");
    }

    private static TestVSTestIntegrationConfigurationResolver CreateResolver() => new TestVSTestIntegrationConfigurationResolver();
        
}
