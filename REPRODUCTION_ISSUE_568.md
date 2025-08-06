# Reproduction of Issue #568: Assert.Ignore in BeforeFeature hook gives System.NullReferenceException

## Issue Summary

**Issue Link**: https://github.com/reqnroll/Reqnroll/issues/568

**Problem**: When using `Assert.Ignore()` (NUnit) or similar exceptions in a `BeforeFeature` hook, a `NullReferenceException` occurs during test teardown instead of graceful handling.

**Original Symptom**:
```
TearDown : System.NullReferenceException : Object reference not set to an instance of an object.
-> warning: The previous ScenarioContext was already disposed.
```

**Fixed in**: PR #560, released in v2.4.1

## Root Cause Analysis

The issue occurred because when a `BeforeFeature` hook failed:

1. The test infrastructure did not properly capture the hook failure 
2. Scenario execution would attempt to continue but with corrupted state
3. During teardown, the code would access null references because the feature context wasn't properly initialized
4. The cleanup logic wasn't protected by try/finally blocks

## Fix Overview (PR #560)

The fix involved several key changes:

1. **Added exception tracking in FeatureContext**:
   - Added `BeforeFeatureHookError` and `BeforeFeatureHookFailed` properties
   - Capture exceptions in `OnFeatureStartAsync` and store them

2. **Enhanced cleanup logic**:
   - Wrapped cleanup operations in try/finally blocks
   - Ensured events are published even if hooks fail
   - Added null checks for scenario context

3. **Improved generated code**:
   - Added proper try/catch/finally blocks in generated test code
   - Enhanced error handling for different programming languages (C#/VB)

## Reproduction Tests Created

### 1. Unit Tests (`BeforeFeatureHookIgnoreReproductionTests.cs`)

Located in: `Tests/Reqnroll.RuntimeTests/BeforeFeatureHookIgnoreReproductionTests.cs`

**Tests included**:
- `Should_handle_ignore_exception_in_before_feature_hook_without_null_reference`
- `Should_handle_pending_step_exception_in_before_feature_hook_without_null_reference`
- `Should_skip_scenario_execution_when_before_feature_hook_failed`
- `Should_properly_cleanup_feature_context_even_when_before_feature_hook_fails`

**Purpose**: Verify that the core test execution engine properly handles exceptions in before feature hooks and doesn't throw NullReferenceException during cleanup.

### 2. Integration Tests (`BeforeFeatureHookIgnoreIntegrationTests.cs`)

Located in: `Tests/Reqnroll.SystemTests/BeforeFeatureHookIgnoreIntegrationTests.cs`

**Tests included**:
- `Should_handle_assert_ignore_in_before_feature_hook_without_null_reference_nunit`
- `Should_handle_pending_step_exception_in_before_feature_hook_without_null_reference`

**Purpose**: End-to-end verification using real test projects with NUnit/MSTest frameworks to ensure the issue is resolved in realistic scenarios.

## Test Scenarios Covered

### Scenario 1: NUnit Assert.Ignore in BeforeFeature Hook
```csharp
[BeforeFeature("Calculator")]
public static void BeforeFeatureToIgnore()
{
    Assert.Ignore("for testing");
}
```

**Expected Behavior**: 
- Test should be marked as ignored
- No NullReferenceException during teardown
- Proper cleanup should occur

### Scenario 2: PendingStepException in BeforeFeature Hook
```csharp
[BeforeFeature]
public static void BeforeFeatureToIgnore()
{
    throw new PendingStepException("Feature is pending");
}
```

**Expected Behavior**:
- Test should fail with PendingStepException
- No infrastructure errors or NullReferenceException
- Proper cleanup should occur

### Scenario 3: Generic Exception in BeforeFeature Hook
```csharp
[BeforeFeature]
public static void BeforeFeatureToIgnore()
{
    throw new InvalidOperationException("Simulated hook failure");
}
```

**Expected Behavior**:
- Test should fail with the original exception
- Exception should be properly captured in FeatureContext.BeforeFeatureHookError
- No secondary NullReferenceException

## Verification Points

All reproduction tests verify:

1. **No NullReferenceException**: The primary symptom should not occur
2. **No ScenarioContext disposal warnings**: Should not see "The previous ScenarioContext was already disposed"
3. **Proper exception tracking**: `FeatureContext.BeforeFeatureHookFailed` should be true when hooks fail
4. **Graceful cleanup**: `CleanupFeatureContext()` should be called exactly once
5. **Scenario execution handling**: When BeforeFeature fails, scenario execution should be properly skipped

## How to Run the Tests

### Unit Tests
```bash
cd /home/runner/work/Reqnroll/Reqnroll
dotnet test Tests/Reqnroll.RuntimeTests/Reqnroll.RuntimeTests.csproj --filter "FullyQualifiedName~ignore_exception_in_before_feature_hook_without_null_reference"
```

### Integration Tests  
```bash
cd /home/runner/work/Reqnroll/Reqnroll
dotnet test Tests/Reqnroll.SystemTests/Reqnroll.SystemTests.csproj --filter "BeforeFeatureHookIgnoreIntegrationTests"
```

### All Related Tests
```bash
cd /home/runner/work/Reqnroll/Reqnroll
dotnet test Tests/Reqnroll.RuntimeTests/Reqnroll.RuntimeTests.csproj --filter "TestExecutionEngineTests"
```

## Current Status

✅ **RESOLVED**: The issue has been fixed in PR #560 and released in v2.4.1.

All reproduction tests pass, confirming that:
- Assert.Ignore in BeforeFeature hooks no longer causes NullReferenceException
- Proper cleanup occurs even when before feature hooks fail
- The infrastructure correctly tracks hook failures
- Scenario execution is properly skipped when feature initialization fails

## Notes for Future Development

1. **Test Coverage**: These reproduction tests should be maintained to prevent regression
2. **Similar Issues**: The fix pattern (try/finally, proper state tracking) should be applied to similar hook failure scenarios
3. **Documentation**: The behavior is now documented in `docs/automation/hooks.md`