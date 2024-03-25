using System;
using Reqnroll.BoDi;
using Reqnroll;

// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow;

public class TestThreadContext(Reqnroll.TestThreadContext originalContext) : ITestThreadContext
{
    public Exception TestError => originalContext.TestError;

    public IObjectContainer TestThreadContainer => originalContext.TestThreadContainer;
}
