; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1001  |  Usage   |  Error   | StepTextAnalyzer_StepTextCannotBeNull, [Documentation](../docs/automation/code-analysis/step-rules/rr1001.md)
RR1002  |  Usage   |  Error   | StepTextAnalyzer_StepTextCannotEmptyOrWhitespaceRule, [Documentation](../docs/automation/code-analysis/step-rules/rr1002.md)
RR1003  |  Usage   |  Error   | StepTextAnalyzer_StepTextShouldNotHaveLeadingOrTrailingWhitespaceRule, [Documentation](../docs/automation/code-analysis/step-rules/rr1003.md)
RR1021  |  Usage   | Warning  | StepMethodReturnTypeAnalyzer, [Documentation](../docs/automation/code-analysis/step-rules/rr1021.md)
RR1022  |  Usage   |  Error   | AsyncStepMethodReturnTypeAnalyzer, [Documentation](../docs/automation/code-analysis/step-rules/rr1022.md)
