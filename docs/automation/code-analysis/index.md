# Code Analysis
Reqnroll includes a Rolsyn code analyser that inspects your C# or Visual Basic code to identify potential problems at design and build-time. The analyser checks code against a set of rules and reports any violations as issues. Reqnroll issues are prefixed with "RR" to separate them from issues produced by other code analysis or the compiler.

```{toctree}
:hidden:

step-definition-rules/index
```

## Configuration
Rules can be configured using the standard methods for Roslyn analyzers:
- [Customize Roslyn analyzer rules](https://learn.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2022)
- [Configure live code analysis for .NET](https://learn.microsoft.com/en-us/visualstudio/code-quality/configure-live-code-analysis-scope-managed-code?view=vs-2022)
- [Suppress code analysis violations](https://learn.microsoft.com/en-us/visualstudio/code-quality/in-source-suppression-overview?view=vs-2022&tabs=csharp)

## Analyzer Rules
The following rules are provided.

| Rule ID | Title |
| --- | --- |
| [RR1001](./step-definition-rules/rr1001.md) | Step text cannot be null |
| [RR1002](./step-definition-rules/rr1002.md) | Step text cannot be empty or whitespace |
| [RR1003](./step-definition-rules/rr1003.md) | Step text cannot have leading or trailing whitespace |
| [RR1021](./step-definition-rules/rr1021.md) | Step methods should not return values |
| [RR1022](./step-definition-rules/rr1022.md) | Asynchronous step methods must return a Task, Task&lt;TResult&gt;, ValueTask or ValueTask&lt;TResult&gt; |