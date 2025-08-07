using FluentAssertions;
using System;
using System.IO;
using System.Xml;
using Xunit;

namespace Reqnroll.GeneratorTests.MSBuildTask
{
    public class MSBuildPropsFileTests
    {
        [Fact]
        public void PropsFile_ShouldUseForwardSlashesInGlobPatterns()
        {
            // ARRANGE
            var xmlDoc = LoadPropsXml();
            
            // ACT & ASSERT - check ReqnrollFeatureFiles and ReqnrollObsoleteCodeBehindFiles Include attributes
            ValidateIncludeAttributesUseForwardSlashes(xmlDoc, "//ReqnrollFeatureFiles[@Include]", "ReqnrollFeatureFiles");
            ValidateIncludeAttributesUseForwardSlashes(xmlDoc, "//ReqnrollObsoleteCodeBehindFiles[@Include]", "ReqnrollObsoleteCodeBehindFiles");
        }
        
        [Fact]
        public void PropsFile_ShouldContainExpectedGlobPatterns()
        {
            // ARRANGE
            var xmlDoc = LoadPropsXml();
            
            // ACT & ASSERT - check specific expected patterns
            var featureFilesNode = xmlDoc.SelectSingleNode("//ReqnrollFeatureFiles[@Include='**/*.feature']");
            featureFilesNode.Should().NotBeNull("ReqnrollFeatureFiles should include '**/*.feature' glob pattern");
            
            var obsoleteCodeBehindNode = xmlDoc.SelectSingleNode("//ReqnrollObsoleteCodeBehindFiles[contains(@Include, '**/*.feature$(DefaultLanguageSourceExtension)')]");
            obsoleteCodeBehindNode.Should().NotBeNull("ReqnrollObsoleteCodeBehindFiles should include pattern with '**/*.feature$(DefaultLanguageSourceExtension)'");
        }

        #region Helper Methods

        /// <summary>
        /// Gets the full path to the Reqnroll.Tools.MsBuild.Generation.props file
        /// </summary>
        /// <returns>Full path to the props file</returns>
        private string GetPropsFilePath()
        {
            var testAssemblyLocation = typeof(MSBuildPropsFileTests).Assembly.Location;
            var testBinDir = Path.GetDirectoryName(testAssemblyLocation);
            var testProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(testBinDir))); // Go up three levels from bin/Debug/net8.0
            var repoRoot = Path.GetDirectoryName(Path.GetDirectoryName(testProjectDir)); // Go up two more levels from Tests/Reqnroll.GeneratorTests
            var fullPath = Path.Combine(repoRoot, "Reqnroll.Tools.MsBuild.Generation", "build", "Reqnroll.Tools.MsBuild.Generation.props");
            
            // Assert that file exists
            File.Exists(fullPath).Should().BeTrue($"props file should exist at {fullPath}");
            
            return fullPath;
        }

        /// <summary>
        /// Loads and parses the MSBuild props XML file
        /// </summary>
        /// <returns>Parsed XML document</returns>
        private XmlDocument LoadPropsXml()
        {
            var fullPath = GetPropsFilePath();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fullPath);
            return xmlDoc;
        }

        /// <summary>
        /// Validates that Include attributes in the specified nodes use forward slashes instead of backslashes
        /// </summary>
        /// <param name="xmlDoc">The XML document to search</param>
        /// <param name="nodeSelector">XPath selector for the nodes to validate</param>
        /// <param name="nodeDescription">Description of the node type for error messages</param>
        private void ValidateIncludeAttributesUseForwardSlashes(XmlDocument xmlDoc, string nodeSelector, string nodeDescription)
        {
            var nodes = xmlDoc.SelectNodes(nodeSelector);
            nodes.Should().NotBeNull();
            nodes.Count.Should().BeGreaterThan(0, $"Should find at least one {nodeDescription} node");
            
            foreach (XmlNode node in nodes)
            {
                var includeAttribute = node.Attributes["Include"];
                includeAttribute.Should().NotBeNull($"{nodeDescription} node should have Include attribute");
                var includeValue = includeAttribute.Value;
                
                // Should use forward slashes, not backslashes
                includeValue.Should().NotContain("\\", $"{nodeDescription} Include should use forward slashes, but found: {includeValue}");
                includeValue.Should().Contain("/", $"{nodeDescription} Include should contain forward slashes for glob patterns, but found: {includeValue}");
            }
        }

        #endregion
    }
}