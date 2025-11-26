using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.RuntimeSupport;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.RuntimeSupport
{
    public class TestRowPickleMapperTests
    {
        private PickleTag CreateTag(string name)
        {
            return new PickleTag(name, "");
        }

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

        private FeatureInfo CreateFeatureInfoWithPickles(IEnumerable<Pickle> pickles)
        {
            var envelopes = pickles.Select(p => Envelope.Create(p)).ToList().AsReadOnly();
            var featureLevelCucumberMessages = new FeatureLevelCucumberMessages(envelopes, envelopes.Count());
            return new FeatureInfo(
                language: System.Globalization.CultureInfo.InvariantCulture,
                tags: Array.Empty<string>(),
                folderPath: "",
                title: "Feature Title",
                description: "",
                programmingLanguage: ProgrammingLanguage.CSharp,
                featureLevelCucumberMessages: featureLevelCucumberMessages);
        }

        [Fact]
        public void ComputeHash_ReturnsConsistentHash_ForSameInputs()
        {
            var hash1 = TestRowPickleMapper.ComputeHash("Feature", "Scenario", new[] { "tag1", "tag2" }, new[] { "val1", "val2" });
            var hash2 = TestRowPickleMapper.ComputeHash("Feature", "Scenario", new[] { "tag1", "tag2" }, new[] { "val1", "val2" });
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void ComputeHash_DiffersForDifferentInputs()
        {
            var hash1 = TestRowPickleMapper.ComputeHash("FeatureA", "Scenario", new[] { "tag1" }, new[] { "val1" });
            var hash2 = TestRowPickleMapper.ComputeHash("FeatureB", "Scenario", new[] { "tag1" }, new[] { "val1" });
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void MarkPickleWithRowHash_AddsRowHashTag()
        {
            var pickle = CreatePickleWithTags(new List<PickleTag>());
            TestRowPickleMapper.MarkPickleWithRowHash(pickle, "Feature", "Scenario", new[] { "tag1" }, new[] { "val1" });
            var expectedHash = TestRowPickleMapper.ComputeHash("Feature", "Scenario", new[] { "tag1" }, new[] { "val1" });
            Assert.Contains(pickle.Tags, t => t.Name == $"@RowHash_{expectedHash}");
        }

        [Fact]
        public void GetPickleIndexFromTestRow_FindsCorrectPickleIndex()
        {
            var pickle1 = CreatePickleWithTags(new List<PickleTag>());
            var pickle2 = CreatePickleWithTags(new List<PickleTag>());
            var pickles = new List<Pickle> { pickle1, pickle2 };
            var fi = CreateFeatureInfoWithPickles(pickles);
            TestRowPickleMapper.MarkPickleWithRowHash(pickle2, "Feature", "Scenario", new[] { "tag1" }, new[] { "val1" });
            var index = TestRowPickleMapper.GetPickleIndexFromTestRow("Feature", "Scenario", new[] { "tag1" }, new[] { "val1" }, fi);
            Assert.Equal("1", index);
        }

        [Fact]
        public void GetPickleIndexFromTestRow_ReturnsNullIfNoMatch()
        {
            var pickle1 = CreatePickleWithTags(new List<PickleTag>());
            var pickle2 = CreatePickleWithTags(new List<PickleTag>());
            var pickles = new List<Pickle> { pickle1, pickle2 };
            var fi = CreateFeatureInfoWithPickles(pickles);

            var index = TestRowPickleMapper.GetPickleIndexFromTestRow("Feature", "Scenario", new[] { "tag1" }, new[] { "val1" }, fi);
            Assert.Null(index);
        }

        [Fact]
        public void GetPickleIndexFromTestRow_RemovesTagAfterFinding()
        {
            var pickle = CreatePickleWithTags(new List<PickleTag>());
            var pickles = new List<Pickle> { pickle };
            var fi = CreateFeatureInfoWithPickles(pickles);

            TestRowPickleMapper.MarkPickleWithRowHash(pickle, "Feature", "Scenario", new[] { "tag1" }, new[] { "val1" });
            var expectedHash = TestRowPickleMapper.ComputeHash("Feature", "Scenario", new[] { "tag1" }, new[] { "val1" });
            var tagName = $"@RowHash_{expectedHash}";
            var index = TestRowPickleMapper.GetPickleIndexFromTestRow("Feature", "Scenario", new[] { "tag1" }, new[] { "val1" }, fi);
            Assert.Equal("0", index);
            Assert.DoesNotContain(pickle.Tags, t => t.Name == tagName);
        }
    }
}