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
            // Find the props file relative to the repository root
            var testAssemblyLocation = typeof(MSBuildPropsFileTests).Assembly.Location;
            var testBinDir = Path.GetDirectoryName(testAssemblyLocation);
            var testProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(testBinDir))); // Go up three levels from bin/Debug/net8.0
            var repoRoot = Path.GetDirectoryName(Path.GetDirectoryName(testProjectDir)); // Go up two more levels from Tests/Reqnroll.GeneratorTests
            var fullPath = Path.Combine(repoRoot, "Reqnroll.Tools.MsBuild.Generation", "build", "Reqnroll.Tools.MsBuild.Generation.props");
            
            // ASSERT that file exists
            File.Exists(fullPath).Should().BeTrue($"props file should exist at {fullPath}");
            
            // ACT - load and parse the XML
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fullPath);
            
            // ASSERT - check ReqnrollFeatureFiles Include attribute
            var featureFilesNodes = xmlDoc.SelectNodes("//ReqnrollFeatureFiles[@Include]");
            featureFilesNodes.Should().NotBeNull();
            featureFilesNodes.Count.Should().BeGreaterThan(0);
            
            foreach (XmlNode node in featureFilesNodes)
            {
                var includeAttribute = node.Attributes["Include"];
                includeAttribute.Should().NotBeNull();
                var includeValue = includeAttribute.Value;
                
                // Should use forward slashes, not backslashes
                includeValue.Should().NotContain("\\", $"ReqnrollFeatureFiles Include should use forward slashes, but found: {includeValue}");
                includeValue.Should().Contain("/", $"ReqnrollFeatureFiles Include should contain forward slashes for glob patterns, but found: {includeValue}");
            }
            
            // ASSERT - check ReqnrollObsoleteCodeBehindFiles Include attribute
            var obsoleteCodeBehindNodes = xmlDoc.SelectNodes("//ReqnrollObsoleteCodeBehindFiles[@Include]");
            obsoleteCodeBehindNodes.Should().NotBeNull();
            obsoleteCodeBehindNodes.Count.Should().BeGreaterThan(0);
            
            foreach (XmlNode node in obsoleteCodeBehindNodes)
            {
                var includeAttribute = node.Attributes["Include"];
                includeAttribute.Should().NotBeNull();
                var includeValue = includeAttribute.Value;
                
                // Should use forward slashes, not backslashes
                includeValue.Should().NotContain("\\", $"ReqnrollObsoleteCodeBehindFiles Include should use forward slashes, but found: {includeValue}");
                includeValue.Should().Contain("/", $"ReqnrollObsoleteCodeBehindFiles Include should contain forward slashes for glob patterns, but found: {includeValue}");
            }
        }
        
        [Fact]
        public void PropsFile_ShouldContainExpectedGlobPatterns()
        {
            // ARRANGE
            // Find the props file relative to the repository root
            var testAssemblyLocation = typeof(MSBuildPropsFileTests).Assembly.Location;
            var testBinDir = Path.GetDirectoryName(testAssemblyLocation);
            var testProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(testBinDir))); // Go up three levels from bin/Debug/net8.0
            var repoRoot = Path.GetDirectoryName(Path.GetDirectoryName(testProjectDir)); // Go up two more levels from Tests/Reqnroll.GeneratorTests
            var fullPath = Path.Combine(repoRoot, "Reqnroll.Tools.MsBuild.Generation", "build", "Reqnroll.Tools.MsBuild.Generation.props");
            
            // ACT - load and parse the XML
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fullPath);
            
            // ASSERT - check specific expected patterns
            var featureFilesNode = xmlDoc.SelectSingleNode("//ReqnrollFeatureFiles[@Include='**/*.feature']");
            featureFilesNode.Should().NotBeNull("ReqnrollFeatureFiles should include '**/*.feature' glob pattern");
            
            var obsoleteCodeBehindNode = xmlDoc.SelectSingleNode("//ReqnrollObsoleteCodeBehindFiles[contains(@Include, '**/*.feature$(DefaultLanguageSourceExtension)')]");
            obsoleteCodeBehindNode.Should().NotBeNull("ReqnrollObsoleteCodeBehindFiles should include pattern with '**/*.feature$(DefaultLanguageSourceExtension)'");
        }
    }
}