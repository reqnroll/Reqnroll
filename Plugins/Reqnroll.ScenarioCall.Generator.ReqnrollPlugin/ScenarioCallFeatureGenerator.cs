using System;
using System.Collections.Generic;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Gherkin.Ast;

namespace Reqnroll.ScenarioCall.Generator.ReqnrollPlugin
{
    public class ScenarioCallFeatureGenerator : IFeatureGenerator
    {
        private readonly IFeatureGenerator _baseGenerator;
        private readonly Dictionary<string, string> _featureFileCache = new();

        public ScenarioCallFeatureGenerator(IFeatureGenerator baseGenerator, ReqnrollDocument document)
        {
            _baseGenerator = baseGenerator;
        }

        public CodeNamespace GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace, out IEnumerable<string> warnings)
        {
            // The document is already parsed at this point, so we need to work at a different level
            // For now, let's just delegate to the base generator
            // We'll implement content preprocessing at the parser level instead
            return _baseGenerator.GenerateUnitTestFixture(document, testClassName, targetNamespace, out warnings);
        }

        // Helper method to preprocess feature file content and expand scenario calls
        public string PreprocessFeatureContent(string originalContent)
        {
            var lines = originalContent.Split('\n');
            var result = new StringBuilder();
            bool inScenario = false;
            string currentFeatureName = null;

            // First pass: extract the current feature name
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("Feature:"))
                {
                    currentFeatureName = trimmedLine.Substring("Feature:".Length).Trim();
                    break;
                }
            }

            // Second pass: process scenario calls
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Track if we're in a scenario
                if (trimmedLine.StartsWith("Scenario:"))
                {
                    inScenario = true;
                    result.AppendLine(line);
                    continue;
                }
                
                // Check for scenario call steps when in a scenario
                if (inScenario && IsScenarioCallStep(trimmedLine))
                {
                    var expandedSteps = ExpandScenarioCall(trimmedLine, currentFeatureName);
                    if (expandedSteps != null)
                    {
                        result.Append(expandedSteps);
                        continue; // Skip the original call step
                    }
                    else
                    {
                        // If expansion failed, add a comment and keep the original step
                        var leadingWhitespace = line.Substring(0, line.Length - line.TrimStart().Length);
                        result.AppendLine($"{leadingWhitespace}# Warning: Could not expand scenario call");
                    }
                }
                
                // Add the original line
                result.AppendLine(line);
            }

            return result.ToString();
        }

        private bool IsScenarioCallStep(string stepText)
        {
            return Regex.IsMatch(stepText, @"(Given|When|Then|And|But)\s+I call scenario ""([^""]+)"" from feature ""([^""]+)""", RegexOptions.IgnoreCase);
        }

        private string ExpandScenarioCall(string callStepLine, string currentFeatureName)
        {
            var match = Regex.Match(callStepLine, @"(Given|When|Then|And|But)\s+I call scenario ""([^""]+)"" from feature ""([^""]+)""", RegexOptions.IgnoreCase);
            if (!match.Success) return null;

            var scenarioName = match.Groups[2].Value;
            var featureName = match.Groups[3].Value;
            var leadingWhitespace = callStepLine.Substring(0, callStepLine.Length - callStepLine.TrimStart().Length);

            try
            {
                var scenarioSteps = FindScenarioSteps(scenarioName, featureName);
                if (scenarioSteps != null && scenarioSteps.Any())
                {
                    var result = new StringBuilder();
                    result.AppendLine($"{leadingWhitespace}# Expanded from scenario call: \"{scenarioName}\" from feature \"{featureName}\"");
                    
                    foreach (var step in scenarioSteps)
                    {
                        result.AppendLine($"{leadingWhitespace}{step}");
                    }
                    
                    return result.ToString();
                }
            }
            catch (Exception ex)
            {
                // Return error comment
                return $"{leadingWhitespace}# Error expanding scenario call: {ex.Message}\n";
            }

            return null;
        }

        private List<string> FindScenarioSteps(string scenarioName, string featureName)
        {
            var featureContent = FindFeatureFileContent(featureName);
            if (featureContent == null) return null;

            var lines = featureContent.Split('\n');
            var steps = new List<string>();
            bool inTargetScenario = false;
            bool foundFeature = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Check if we're in the right feature
                if (trimmedLine.StartsWith("Feature:"))
                {
                    var currentFeatureName = trimmedLine.Substring("Feature:".Length).Trim();
                    foundFeature = string.Equals(currentFeatureName, featureName, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (!foundFeature) continue;

                // Check for target scenario
                if (trimmedLine.StartsWith("Scenario:"))
                {
                    var currentScenarioName = trimmedLine.Substring("Scenario:".Length).Trim();
                    inTargetScenario = string.Equals(currentScenarioName, scenarioName, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                // Stop if we hit another scenario or feature
                if (inTargetScenario && (trimmedLine.StartsWith("Scenario:") || trimmedLine.StartsWith("Feature:")))
                {
                    break;
                }

                // Collect steps from target scenario
                if (inTargetScenario && IsStepLine(trimmedLine))
                {
                    steps.Add(trimmedLine);
                }
            }

            return steps.Any() ? steps : null;
        }

        private bool IsStepLine(string line)
        {
            return line.StartsWith("Given ") || 
                   line.StartsWith("When ") || 
                   line.StartsWith("Then ") || 
                   line.StartsWith("And ") || 
                   line.StartsWith("But ");
        }

        private string FindFeatureFileContent(string featureName)
        {
            // Check cache first
            if (_featureFileCache.TryGetValue(featureName, out var cachedContent))
            {
                return cachedContent;
            }

            // Search for feature files
            var currentDirectory = Environment.CurrentDirectory;
            var featureFiles = GetFeatureFilePaths(currentDirectory);

            foreach (var featureFile in featureFiles)
            {
                try
                {
                    var content = File.ReadAllText(featureFile);
                    var extractedFeatureName = ExtractFeatureNameFromContent(content);
                    
                    if (extractedFeatureName != null)
                    {
                        _featureFileCache[extractedFeatureName] = content;
                        if (string.Equals(extractedFeatureName, featureName, StringComparison.OrdinalIgnoreCase))
                        {
                            return content;
                        }
                    }
                }
                catch
                {
                    // Continue with next file
                }
            }

            return null;
        }

        private string ExtractFeatureNameFromContent(string content)
        {
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("Feature:"))
                {
                    return trimmedLine.Substring("Feature:".Length).Trim();
                }
            }
            return null;
        }

        private IEnumerable<string> GetFeatureFilePaths(string baseDirectory)
        {
            var featureFiles = new List<string>();

            // Common feature file locations
            var searchPaths = new[]
            {
                baseDirectory,
                Path.Combine(baseDirectory, "Features"),
                Path.Combine(baseDirectory, "Specs"),
                Path.Combine(baseDirectory, "Tests")
            };

            foreach (var searchPath in searchPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    featureFiles.AddRange(Directory.GetFiles(searchPath, "*.feature", SearchOption.AllDirectories));
                }
            }

            return featureFiles.Distinct();
        }
    }
}