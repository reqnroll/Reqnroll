using Reqnroll.BoDi;
using System;

namespace Reqnroll.MSTest.ReqnrollPlugin;

//NOTE: The implementation of this interface depends on the MSTest version used in the test project (MSTest v2-3 vs MSTest v4)
//In order to make it compatible with both versions, the implementation is generated into the project as source code via MSTest.AssemblyHooks.template.cs/.vb.

public interface IMsTestRuntimeAdapter
{
    void RegisterGlobalTestContext(IObjectContainer container);
    object ResolveTestContext(IObjectContainer container);
    void TestContextWriteLine(object testContext, string message);
    void TestContextAddResultFile(object testContext, string filePath);
    void ThrowAssertInconclusiveException(string message);
    bool IsInconclusiveException(Exception exception);
}
