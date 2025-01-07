'<auto-generated />

Imports NUnit.Framework
Imports Reqnroll
Imports System
Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.Runtime.CompilerServices

<GeneratedCode("Reqnroll", "REQNROLL_VERSION")>
<FixtureLifeCycle(NUnit.Framework.LifeCycle.InstancePerTestCase)>
<SetUpFixture>
Public NotInheritable Class PROJECT_ROOT_NAMESPACE_NUnitAssemblyHooks
    <OneTimeSetUp>
    <MethodImpl(MethodImplOptions.NoInlining)>
    Public Shared Async Function AssemblyInitializeAsync() As Task
        Dim currentAssembly As Assembly = GetType(PROJECT_ROOT_NAMESPACE_NUnitAssemblyHooks).Assembly
        Await Global.Reqnroll.TestRunnerManager.OnTestRunStartAsync(currentAssembly)
    End Function

    <OneTimeTearDown>
    <MethodImpl(MethodImplOptions.NoInlining)>
    Public Shared Async Function AssemblyCleanupAsync() As Task
        Dim currentAssembly As Assembly = GetType(PROJECT_ROOT_NAMESPACE_NUnitAssemblyHooks).Assembly
        Await Global.Reqnroll.TestRunnerManager.OnTestRunEndAsync(currentAssembly)
    End Function

End Class
