; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1001  |  Usage   |  Error   | StepTextAnalyzer_StepTextCannotBeNullOrEmptyRule
RR1002  |  Usage   |  Error   | StepMethodMustReturnVoidOrTaskAnalyzer
RR1003  |  Usage   |  Error   | AsyncStepMethodMustReturnTaskAnalyzer
RR1004  |  Usage   | Warning  | StepTextAnalyzer_StepTextShouldNotHaveLeadingWhitespaceRule
RR1005  |  Usage   | Warning  | StepTextAnalyzer_StepTextShouldNotHaveTrailingWhitespaceRule
