using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.BoDi;
using System;
using System.Linq;
using System.Reflection;

namespace Reqnroll.MSTest.ReqnrollPlugin;

public interface IMsTestRuntimeAdapter
{
    void RegisterGlobalTestContext(IObjectContainer container);
    object ResolveTestContext(IObjectContainer container);
    void TestContextWriteLine(object testContext, string message);
    void TestContextAddResultFile(object testContext, string filePath);
    Exception CreateAssertInconclusiveException(string message);
    bool IsInconclusiveException(Exception exception);
}

public class MsTestV3RuntimeAdapter(TestContext globalTestContext) : IMsTestRuntimeAdapter
{
    public MsTestV3RuntimeAdapter(object testContext) : this((TestContext)testContext)
    {
    }

    public void RegisterGlobalTestContext(IObjectContainer container)
    {
        container.RegisterInstanceAs(globalTestContext);
    }

    public object ResolveTestContext(IObjectContainer container) => container.Resolve<TestContext>();

    public void TestContextWriteLine(object testContext, string message)
    {
        ((TestContext)testContext).WriteLine(message);
    }

    public void TestContextAddResultFile(object testContext, string filePath)
    {
        ((TestContext)testContext).AddResultFile(filePath);
    }

    public Exception CreateAssertInconclusiveException(string message) => new AssertInconclusiveException(message);

    public bool IsInconclusiveException(Exception exception) => exception is AssertInconclusiveException;
}

public class MsTestV4ReflectionRuntimeAdapter : IMsTestRuntimeAdapter
{
    private readonly Lazy<Type> _testContextType;
    private readonly Lazy<Type> _assertInconclusiveExceptionType;
    private readonly Lazy<MethodInfo> _testContextWriteLineMethod;
    private readonly Lazy<MethodInfo> _testContextAddResultFileMethod;
    private readonly object _globalTestContext;

    public MsTestV4ReflectionRuntimeAdapter(object globalTestContext)
    {
        _globalTestContext = globalTestContext;
        _testContextType = new Lazy<Type>(() => GetTestContextType(globalTestContext));
        _testContextWriteLineMethod = new Lazy<MethodInfo>(() => GetTestContextWriteLineMethod(_testContextType.Value));
        _testContextAddResultFileMethod = new Lazy<MethodInfo>(() => GetTestContextAddResultFileMethod(_testContextType.Value));
        _assertInconclusiveExceptionType = new Lazy<Type>(GetAssertInconclusiveExceptionType);
    }

    public void RegisterGlobalTestContext(IObjectContainer container)
    {
        container.RegisterInstanceAs(_globalTestContext, _testContextType.Value);
    }

    public object ResolveTestContext(IObjectContainer container) => container.Resolve(_testContextType.Value);

    public void TestContextWriteLine(object testContext, string message)
    {
        _testContextWriteLineMethod.Value.Invoke(testContext, [message]);
    }

    public void TestContextAddResultFile(object testContext, string filePath)
    {
        _testContextAddResultFileMethod.Value.Invoke(testContext, [filePath]);
    }

    public Exception CreateAssertInconclusiveException(string message) =>
        (Exception)Activator.CreateInstance(_assertInconclusiveExceptionType.Value, message);

    public bool IsInconclusiveException(Exception exception) => exception.GetType().Name == nameof(AssertInconclusiveException);

    private static Type GetTestContextType(object context)
    {
        var type = context.GetType();
        while (type != null && type.Name != nameof(TestContext)) type = type.BaseType;
        if (type == null)
            throw new InvalidOperationException("Could not find MSTest V4 TestContext type.");
        return type;
    }

    private static MethodInfo GetTestContextWriteLineMethod(Type testContextType)
    {
        var methodInfo = testContextType.GetMethod(nameof(TestContext.WriteLine), BindingFlags.Public | BindingFlags.Instance, null, [typeof(string)], null);
        if (methodInfo == null)
            throw new InvalidOperationException("Could not find WriteLine method on MSTest V4 TestContext type.");
        return methodInfo;
    }

    private static MethodInfo GetTestContextAddResultFileMethod(Type testContextType)
    {
        var methodInfo = testContextType.GetMethod(nameof(TestContext.AddResultFile), BindingFlags.Public | BindingFlags.Instance, null, [typeof(string)], null);
        if (methodInfo == null)
            throw new InvalidOperationException("Could not find AddResultFile method on MSTest V4 TestContext type.");
        return methodInfo;
    }

    private static Type GetAssertInconclusiveExceptionType()
    {
        var frameworkAssembly = AppDomain.CurrentDomain.GetAssemblies()
                          .FirstOrDefault(a => a.GetName().Name == "MSTest.TestFramework");
        if (frameworkAssembly == null)
            throw new InvalidOperationException("Could not find MSTest MSTest.TestFramework assembly in loaded assemblies.");

        var type = frameworkAssembly.GetType("Microsoft.VisualStudio.TestTools.UnitTesting.AssertInconclusiveException");
        if (type == null)
            throw new InvalidOperationException("Could not find MSTest AssertInconclusiveException type in loaded assemblies.");
        return type;
    }
}

public static class MsTestRuntimeAdapterSelector
{
    public static IMsTestRuntimeAdapter GetAdapter(object testContext)
    {
        if (testContext.GetType().Assembly.GetName().Name == "Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices")
        {
            return new MsTestV3RuntimeAdapter(testContext);
        }
        else
        {
            return new MsTestV4ReflectionRuntimeAdapter(testContext);
        }
    }
}
