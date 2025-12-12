; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1000  |  Usage   |  Error   | StepTextAnalyzer_StepTextCannotBeNullOrEmptyRule
RR1001  |  Usage   | Warning  | StepTextAnalyzer_StepTextShouldNotHaveLeadingWhitespaceRule
RR1002  |  Usage   | Warning  | StepTextAnalyzer_StepTextShouldNotHaveTrailingWhitespaceRule
RR1003  |  Usage   | Warning  | StepMethodReturnTypeAnalyzer
RR1004  |  Usage   |  Error   | AsyncStepMethodReturnTypeAnalyzer, [Documentation]()
