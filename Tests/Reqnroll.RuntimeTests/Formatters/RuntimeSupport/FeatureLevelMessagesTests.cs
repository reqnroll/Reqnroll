using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.RuntimeSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.RuntimeSupport;

public class FeatureLevelMessagesTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidResourceName_ShouldInitializeCorrectly()
    {
        // Act
        var sut = new FeatureLevelCucumberMessages("test/resource", 3);

        // Assert
        sut.Should().NotBeNull();
        sut.ExpectedEnvelopeCount.Should().Be(3);
    }

    [Fact]
    public void Constructor_WithNullResourceName_ShouldCreateDisabled()
    {
        // Act
        var sut = new FeatureLevelCucumberMessages((string)null, 1);

        // Assert
        sut.HasMessages.Should().BeFalse();
        sut.GherkinDocument.Should().BeNull();
        sut.Source.Should().BeNull();
        sut.Pickles.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyResourceName_ShouldCreateDisabled()
    {
        // Act
        var sut = new FeatureLevelCucumberMessages("", 1);

        // Assert
        sut.HasMessages.Should().BeFalse();
        sut.GherkinDocument.Should().BeNull();
        sut.Source.Should().BeNull();
        sut.Pickles.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithZeroEnvelopeCount_ShouldCreateDisabled()
    {
        // Act
        var sut = new FeatureLevelCucumberMessages("xxx", 0);

        // Assert
        sut.HasMessages.Should().BeFalse();
        sut.GherkinDocument.Should().BeNull();
        sut.Source.Should().BeNull();
        sut.Pickles.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Constructor_WithVariousEnvelopeCounts_ShouldInitializeWithGivenCount(int envelopeCount)
    {
        // Act
        var sut = new FeatureLevelCucumberMessages("test/resource", envelopeCount);

        // Assert
        sut.ExpectedEnvelopeCount.Should().Be(envelopeCount);
    }

    #endregion

    #region Internal Constructor Tests

    [Fact]
    public void InternalConstructor_WithValidEnvelopes_ShouldInitializeCorrectly()
    {
        // Arrange
        var envelopes = new[]
        {
            Envelope.Create(Source1),
            Envelope.Create(GherkinDocument1),
            Envelope.Create(Pickle1)
        };

        // Act
        var sut = new FeatureLevelCucumberMessages(envelopes, 3);

        // Assert
        sut.Should().NotBeNull();
        sut.ExpectedEnvelopeCount.Should().Be(3);
    }

    [Fact]
    public void InternalConstructor_WithStream_ShouldInitializeCorrectly()
    {
        // Arrange
        var ndjsonContent = """
            {"source":{"uri":"Features/Calculator.feature","data":"Feature: Calculator\r\n\r\nSimple calculator for adding two numbers\r\n\r\n@mytag\r\nScenario: Add two numbers\r\n\tGiven the first number is 50\r\n\tAnd the second number is 70\r\n\tWhen the two numbers are added\r\n\tThen the result should be 120","mediaType":"text/x.cucumber.gherkin+plain"}}
            {"gherkinDocument":{"uri":"Features/Calculator.feature","feature":{"location":{"line":1,"column":1},"tags":[],"language":"en-US","keyword":"Feature","name":"Calculator","description":"Simple calculator for adding two numbers","children":[{"scenario":{"location":{"line":6,"column":1},"tags":[{"location":{"line":5,"column":1},"name":"@mytag","id":"9faee3c6b313125e9c77bf04c9d75fa6"}],"keyword":"Scenario","name":"Add two numbers","description":"","steps":[{"location":{"line":7,"column":2},"keyword":"Given ","keywordType":"Context","text":"the first number is 50","id":"e2aa567c918dee5c9b2a6e777fad585d"},{"location":{"line":8,"column":2},"keyword":"And ","keywordType":"Conjunction","text":"the second number is 70","id":"edea8fd465cbb65aabe0a4c97ccee706"},{"location":{"line":9,"column":2},"keyword":"When ","keywordType":"Action","text":"the two numbers are added","id":"bada27e1f4687750a07d3e3036445756"},{"location":{"line":10,"column":2},"keyword":"Then ","keywordType":"Outcome","text":"the result should be 120","id":"645176625268df5f9cfda379a4659a15"}],"examples":[],"id":"d332a3eb5951985da32f78ff459d7a01"}}]},"comments":[]}}
            {"pickle":{"id":"0259248d2f62945ba13d9b7a60dc5067","uri":"Features/Calculator.feature","name":"Add two numbers","language":"en-US","steps":[{"astNodeIds":["e2aa567c918dee5c9b2a6e777fad585d"],"id":"76eb5e15998d265d85784fac7e69c618","type":"Context","text":"the first number is 50"},{"astNodeIds":["edea8fd465cbb65aabe0a4c97ccee706"],"id":"5fcf82f556cda75db7fb346cbbe54288","type":"Context","text":"the second number is 70"},{"astNodeIds":["bada27e1f4687750a07d3e3036445756"],"id":"e7025e92466f2458b61df37f06d3177b","type":"Action","text":"the two numbers are added"},{"astNodeIds":["645176625268df5f9cfda379a4659a15"],"id":"dcd4c3ccdce4665fa795fb8e69feedb6","type":"Outcome","text":"the result should be 120"}],"tags":[{"name":"@mytag","astNodeId":"9faee3c6b313125e9c77bf04c9d75fa6"}],"astNodeIds":["d332a3eb5951985da32f78ff459d7a01"]}}
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ndjsonContent));

        // Act
        var sut = new FeatureLevelCucumberMessages(stream, "test", 2);

        // Assert
        sut.Should().NotBeNull();
        sut.ExpectedEnvelopeCount.Should().Be(2);
    }

    #endregion

    #region ReadEnvelopesFromStream Tests

    [Fact]
    public void ReadEnvelopesFromStream_WithValidNdJson_ShouldParseCorrectly()
    {
        // Arrange
        var ndjsonContent = """
            {"source":{"uri":"Features/Calculator.feature","data":"Feature: Calculator\r\n\r\nSimple calculator for adding two numbers\r\n\r\n@mytag\r\nScenario: Add two numbers\r\n\tGiven the first number is 50\r\n\tAnd the second number is 70\r\n\tWhen the two numbers are added\r\n\tThen the result should be 120","mediaType":"text/x.cucumber.gherkin+plain"}}
            {"gherkinDocument":{"uri":"Features/Calculator.feature","feature":{"location":{"line":1,"column":1},"tags":[],"language":"en-US","keyword":"Feature","name":"Calculator","description":"Simple calculator for adding two numbers","children":[{"scenario":{"location":{"line":6,"column":1},"tags":[{"location":{"line":5,"column":1},"name":"@mytag","id":"9faee3c6b313125e9c77bf04c9d75fa6"}],"keyword":"Scenario","name":"Add two numbers","description":"","steps":[{"location":{"line":7,"column":2},"keyword":"Given ","keywordType":"Context","text":"the first number is 50","id":"e2aa567c918dee5c9b2a6e777fad585d"},{"location":{"line":8,"column":2},"keyword":"And ","keywordType":"Conjunction","text":"the second number is 70","id":"edea8fd465cbb65aabe0a4c97ccee706"},{"location":{"line":9,"column":2},"keyword":"When ","keywordType":"Action","text":"the two numbers are added","id":"bada27e1f4687750a07d3e3036445756"},{"location":{"line":10,"column":2},"keyword":"Then ","keywordType":"Outcome","text":"the result should be 120","id":"645176625268df5f9cfda379a4659a15"}],"examples":[],"id":"d332a3eb5951985da32f78ff459d7a01"}}]},"comments":[]}}
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ndjsonContent));
        var sut = new FeatureLevelCucumberMessages(Array.Empty<Envelope>(), 0);

        // Act
        var result = sut.ReadEnvelopesFromStream(stream);

        // Assert
        result.Should().HaveCount(2);
        result.First().Source.Should().NotBeNull();
        result.Last().GherkinDocument.Should().NotBeNull();
    }

    [Fact]
    public void ReadEnvelopesFromStream_WithMalformedJson_ShouldReturnEmptyCollection()
    {
        // Arrange
        var malformedJson = "{ invalid json content }";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(malformedJson));
        var sut = new FeatureLevelCucumberMessages(Array.Empty<Envelope>(), 0);

        // Act
        var result = sut.ReadEnvelopesFromStream(stream);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ReadEnvelopesFromStream_WithEmptyStream_ShouldReturnEmptyCollection()
    {
        // Arrange
        using var stream = new MemoryStream();
        var sut = new FeatureLevelCucumberMessages(Array.Empty<Envelope>(), 0);

        // Act
        var result = sut.ReadEnvelopesFromStream(stream);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void ReadEnvelopesFromAssembly_WithNonexistentResource_ShouldReturnEmptyCollection()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var sut = new FeatureLevelCucumberMessages(Array.Empty<Envelope>(), 0);

        // Act
        var result = sut.ReadEnvelopesFromAssembly(assembly, "nonexistent/resource");

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Property Access Tests

    [Fact]
    public void Source_WithValidSourceEnvelope_ShouldReturnSource()
    {
        // Arrange
        var expectedSource = Source1;
        var envelopes = new[] { Envelope.Create(expectedSource) };
        var sut = new FeatureLevelCucumberMessages(envelopes, 1);

        // Act
        var result = sut.Source;

        // Assert
        result.Should().BeSameAs(expectedSource);
    }

    [Fact]
    public void Source_WithNoSourceEnvelopes_ShouldReturnNull()
    {
        // Arrange
        var envelopes = new[] { Envelope.Create(GherkinDocument1) };
        var sut = new FeatureLevelCucumberMessages(envelopes, 1);

        // Act
        var result = sut.Source;

        // Assert
        result.Should().BeNull();
    }


    [Fact]
    public void GherkinDocument_WithNoGherkinEnvelopes_ShouldReturnNull()
    {
        // Arrange
        var envelopes = new[] { Envelope.Create(Source1) };
        var sut = new FeatureLevelCucumberMessages(envelopes, 1);

        // Act
        var result = sut.GherkinDocument;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Pickles_WithMultiplePickleEnvelopes_ShouldReturnAllPickles()
    {
        // Arrange
        var pickle1 = Pickle1;
        var pickle2 = Pickle2;
        var envelopes = new[]
        {
            Envelope.Create(pickle1 ),
            Envelope.Create(pickle2 ),
            Envelope.Create(Source1 ) // Mixed with other types
        };
        var sut = new FeatureLevelCucumberMessages(envelopes, 3);

        // Act
        var result = sut.Pickles.ToList();

        // Assert
        result.Should().HaveCount(2)
            .And.Contain(pickle1)
            .And.Contain(pickle2);
    }

    [Fact]
    public void Pickles_WithNoPickleEnvelopes_ShouldReturnEmptyCollection()
    {
        // Arrange
        var envelopes = new[]
        {
            Envelope.Create(Source1),
            Envelope.Create(GherkinDocument1),
        };
        var sut = new FeatureLevelCucumberMessages(envelopes, 2);

        // Act
        var result = sut.Pickles.ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region HasMessages Property Tests

    [Fact]
    public void HasMessages_WithSourceGherkinAndExpectedPickleCount_ShouldReturnTrue()
    {
        // Arrange
        var envelopes = new[]
        {
            Envelope.Create(Source1),
            Envelope.Create(GherkinDocument1),
            Envelope.Create(Pickle1)
    };
        var sut = new FeatureLevelCucumberMessages(envelopes, 3); // 2 + 1 pickle = 3

        // Act & Assert
        sut.HasMessages.Should().BeTrue();
    }

    [Fact]
    public void HasMessages_WithMissingSource_ShouldReturnFalse()
    {
        // Arrange
        var envelopes = new[]
        {
            Envelope.Create(GherkinDocument1),
            Envelope.Create(Pickle1)
        };
        var sut = new FeatureLevelCucumberMessages(envelopes, 2);

        // Act & Assert
        sut.HasMessages.Should().BeFalse();
    }

    [Fact]
    public void HasMessages_WithMissingGherkinDocument_ShouldReturnFalse()
    {
        // Arrange
        var envelopes = new[]
        {
            Envelope.Create(Source1),
            Envelope.Create(Pickle1)
        };
        var sut = new FeatureLevelCucumberMessages(envelopes, 2);

        // Act & Assert
        sut.HasMessages.Should().BeFalse();
    }

    [Fact]
    public void HasMessages_WithIncorrectEnvelopeCount_ShouldReturnFalse()
    {
        // Arrange
        var envelopes = new[]
        {
            Envelope.Create(Source1),
            Envelope.Create(GherkinDocument1),
            Envelope.Create(Pickle1)
        };
        var sut = new FeatureLevelCucumberMessages(envelopes, 5); // Expected 5, actual 3

        // Act & Assert
        sut.HasMessages.Should().BeFalse();
    }

    [Fact]
    public void HasMessages_WithEmptyEnvelopes_ShouldReturnFalse()
    {
        // Arrange
        var sut = new FeatureLevelCucumberMessages(Array.Empty<Envelope>(), 0);

        // Act & Assert
        sut.HasMessages.Should().BeFalse();
    }

    #endregion
    private Source Source1 => new("Source1URI", "Feature 1", SourceMediaType.TEXT_X_CUCUMBER_GHERKIN_PLAIN);
    private GherkinDocument GherkinDocument1 => new("URI1", new Feature(new Location(0,0), new List<Tag>(), "en", "Feature", "gherkin doc", "", new List<FeatureChild>()), new List<Comment>());
    private Pickle Pickle1 => new("pickleId1", "URI1", "pickle 1", "en", new List<PickleStep>(), new List<PickleTag>(), new List<string>());
    private Pickle Pickle2 => new("pickleId2", "URI2", "pickle 2", "en", new List<PickleStep>(), new List<PickleTag>(), new List<string>());
}
