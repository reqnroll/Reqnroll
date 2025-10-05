#nullable enable
using System;
using System.Collections.Generic;
using Reqnroll.CommonModels;

namespace Reqnroll.EnvironmentAccess;

public class EnvironmentOptions(IEnvironmentWrapper environment) : IEnvironmentOptions
{
    public const string REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE = "REQNROLL_FORMATTERS";
    public const string REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX = "REQNROLL_FORMATTERS_";
    public const string REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE = "REQNROLL_FORMATTERS_DISABLED";
    public const string REQNROLL_DRY_RUN_ENVIRONMENT_VARIABLE = "REQNROLL_DRY_RUN";
    public const string REQNROLL_BINDING_OUTPUT_ENVIRONMENT_VARIABLE = "REQNROLL_BINDING_OUTPUT";
    public const string DOTNET_RUNNING_IN_CONTAINER_ENVIRONMENT_VARIABLE = "DOTNET_RUNNING_IN_CONTAINER";

    private readonly Lazy<bool> _isDryRunLazy = new Lazy<bool>(() =>
        environment.GetEnvironmentVariable(REQNROLL_DRY_RUN_ENVIRONMENT_VARIABLE) is ISuccess<string> dryRunVar
            && bool.TryParse(dryRunVar.Result, out bool isDryRun)
            && isDryRun);

    public bool IsDryRun => _isDryRunLazy.Value;

    public string? BindingsOutputFilepath => 
        environment.GetEnvironmentVariable(REQNROLL_BINDING_OUTPUT_ENVIRONMENT_VARIABLE) is ISuccess<string> outputVar
            ? outputVar.Result
            : null;

    public bool IsRunningInContainer => environment.IsEnvironmentVariableSet(DOTNET_RUNNING_IN_CONTAINER_ENVIRONMENT_VARIABLE);

    public bool FormattersDisabled => 
        environment.GetEnvironmentVariable(REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE) is ISuccess<string> disabledVar
            && bool.TryParse(disabledVar.Result, out bool disabled)
            && disabled;

    public string? FormattersJson => 
        environment.GetEnvironmentVariable(REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE) is ISuccess<string> formattersVar 
            ? formattersVar.Result
            : null;

    public IDictionary<string, string> FormatterSettings =>
        environment.GetEnvironmentVariables(REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX, trimPrefix: true);
}
