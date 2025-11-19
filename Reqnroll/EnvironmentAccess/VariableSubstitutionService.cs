using Reqnroll.CommonModels;
using Reqnroll.Time;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Reqnroll.EnvironmentAccess;

/// <summary>
/// Provides functionality to resolve template strings by substituting placeholders with environment variables or
/// built-in values.
/// </summary>
/// <remarks>This service supports replacing placeholders in the format "{env:VARIABLE_NAME}" with the
/// corresponding environment variable value, and "{timestamp}" with the current date and time,
/// or one of {buildNumber}, {revision}, {branch}, or {tag} with the corresponding information from BuildMetadata. 
/// Placeholders that do not match a known variable or environment variable are left unchanged. 
/// This class is typically used to dynamically generate strings such as file paths or configuration values that depend on runtime context.
/// </remarks>
public class VariableSubstitutionService : IVariableSubstitutionService
{
    private readonly IEnvironmentWrapper _environmentWrapper;
    private readonly IClock _clock;
    private readonly BuildMetadata _buildMetadata;

    public VariableSubstitutionService(IEnvironmentWrapper environmentWrapper, IClock clock, IBuildMetadataProvider buildMetadataProvider)
    {
        _environmentWrapper = environmentWrapper ?? throw new ArgumentNullException(nameof(environmentWrapper));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _buildMetadata = buildMetadataProvider?.GetBuildMetadata();
    }
    public string ResolveTemplatePlaceholders(string template)
    {
        var substitutionMatcher = new Regex(@"\{((?:env:)?[a-zA-Z0-9_]+)\}", RegexOptions.Compiled);
        if (string.IsNullOrWhiteSpace(template))
            return template;

        var resolvedOutputFilePath = substitutionMatcher.Replace(template, match =>
        {
            var variableName = match.Groups[1].Value;
            var resolved = ReplaceBuiltInVariable(variableName);
            return resolved ?? match.Value;
        });

        return resolvedOutputFilePath;
    }

    private string ReplaceBuiltInVariable(string variableName)
    {
        if (string.IsNullOrEmpty(variableName))
            return null;

        var builtinVariableResolvers = new Dictionary<string, Func<string>>()
            {
                { "timestamp", () => _clock.GetNowDateAndTime().ToString("yyyy-MM-dd_HH-mm-ss") },
                { "buildNumber", () => _buildMetadata.BuildNumber },
                { "revision", () => _buildMetadata.Revision },
                { "branch", () => _buildMetadata.Branch },
                { "tag", () => _buildMetadata.Tag }
            };

        if (builtinVariableResolvers.TryGetValue(variableName, out var resolver))
        {
            return resolver();
        }

        if (variableName.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
        {
            var envVarName = variableName.Substring("env:".Length).Trim();
            var environmentVariable = _environmentWrapper.GetEnvironmentVariable(envVarName);
            if (environmentVariable is ISuccess<string> ev)
                return ev.Result;
        }
        return null;
    }
}