using System.Collections.Generic;
using System.Linq;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.RuntimeSupport;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.RuntimeSupport;

public class TestRowPickleMapperTests
{
    private Pickle CreatePickleWithTags(IEnumerable<PickleTag> tags)
    {
        return new Pickle(
            id: "id",
            name: "name",
            steps: new List<PickleStep>(),
            tags: tags.ToList(),
            astNodeIds: new List<string>(),
            uri: "",
            language: ""
        );
    }

    [Fact]
    public void ComputeHash_ReturnsConsistentHash_ForSameInputs()
    {
        var hash1 = TestRowPickleMapper.ComputeHash("Feature", "Scenario", ["tag1", "tag2"], ["val1", "val2"]);
        var hash2 = TestRowPickleMapper.ComputeHash("Feature", "Scenario", ["tag1", "tag2"], ["val1", "val2"]);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_DiffersForDifferentInputs()
    {
        var hash1 = TestRowPickleMapper.ComputeHash("FeatureA", "Scenario", ["tag1"], ["val1"]);
        var hash2 = TestRowPickleMapper.ComputeHash("FeatureB", "Scenario", ["tag1"], ["val1"]);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void MarkPickleWithRowHash_AddsRowHashTag()
    {
        var pickle = CreatePickleWithTags(new List<PickleTag>());
        TestRowPickleMapper.MarkPickleWithRowHash(pickle,  "Feature", "Scenario", ["tag1"], ["val1"]);
        var expectedHash = TestRowPickleMapper.ComputeHash("Feature", "Scenario", ["tag1"], ["val1"]);
        Assert.Contains(pickle.Tags, t => t.Name == $"{TestRowPickleMapper.RowHashTagPrefix}{expectedHash}");
    }

    [Fact]
    public void PickleHasRowHashMarkerTag_ReturnsTrueAndHash_WhenTagPresent()
    {
        var pickle = CreatePickleWithTags(new List<PickleTag>());
        TestRowPickleMapper.MarkPickleWithRowHash(pickle, "Feature", "Scenario", ["tag1"], ["val1"]);
        var result = TestRowPickleMapper.PickleHasRowHashMarkerTag(pickle, out var hash);
        var expectedHash = TestRowPickleMapper.ComputeHash("Feature", "Scenario", ["tag1"], ["val1"]);
        Assert.True(result);
        Assert.Equal(expectedHash, hash);
    }

    [Fact]
    public void PickleHasRowHashMarkerTag_ReturnsFalse_WhenTagAbsent()
    {
        var pickle = CreatePickleWithTags(new List<PickleTag>());
        var result = TestRowPickleMapper.PickleHasRowHashMarkerTag(pickle, out var hash);
        Assert.False(result);
        Assert.Null(hash);
    }

    [Fact]
    public void RemoveHashRowMarkerTag_RemovesTag()
    {
        var pickle = CreatePickleWithTags(new List<PickleTag>());
        TestRowPickleMapper.MarkPickleWithRowHash(pickle, "Feature", "Scenario", ["tag1"], ["val1"]);
        TestRowPickleMapper.RemoveHashRowMarkerTag(pickle);
        Assert.DoesNotContain(pickle.Tags, t => t.Name.StartsWith(TestRowPickleMapper.RowHashTagPrefix));
    }
}