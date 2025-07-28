using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.ScenarioCall.Generator.ReqnrollPlugin
{
    public class ScenarioCallTestGenerator : TestGenerator
    {
        private readonly Dictionary<string, string> _featureFileCache = new();

        public ScenarioCallTestGenerator(
            ReqnrollConfiguration reqnrollConfiguration,
            ProjectSettings projectSettings,
            ITestHeaderWriter testHeaderWriter,
            ITestUpToDateChecker testUpToDateChecker,
            IFeatureGeneratorRegistry featureGeneratorRegistry,
            CodeDomHelper codeDomHelper,
            IGherkinParserFactory gherkinParserFactory)
            : base(reqnrollConfiguration, projectSettings, testHeaderWriter, testUpToDateChecker, featureGeneratorRegistry, codeDomHelper, gherkinParserFactory)
        {
        }

        protected override ReqnrollDocument ParseContent(IGherkinParser parser, TextReader contentReader, ReqnrollDocumentLocation documentLocation)
        {
            // Read the original content
            var originalContent = contentReader.ReadToEnd();

            // Preprocess to expand scenario calls
            var expandedContent = PreprocessFeatureContent(originalContent);

            // Parse the expanded content
            using (var expandedReader = new StringReader(expandedContent))
            {
                return parser.Parse(expandedReader, documentLocation);
            }
        }

        private string PreprocessFeatureContent(string originalContent)
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
                
                // Reset scenario flag when we encounter a new feature or background
                if (trimmedLine.StartsWith("Feature:") || trimmedLine.StartsWith("Background:"))
                {
                    inScenario = false;
                    result.AppendLine(line);
                    continue;
                }
                
                // Check for scenario call steps when in a scenario
                if (inScenario && IsScenarioCallStep(trimmedLine))
                {
                    var expandedSteps = ExpandScenarioCall(line, currentFeatureName);
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