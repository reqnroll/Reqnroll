namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Discriminates the two kinds of tracked test-case steps for identity/occurrence keying.
/// </summary>
public enum StepKind
{
    TestStep,
    Hook
}
