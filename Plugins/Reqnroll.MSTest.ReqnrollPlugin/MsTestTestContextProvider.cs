using System;
using Reqnroll.BoDi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.Infrastructure;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    public interface IMSTestTestContextProvider
    {
        void WriteLine(string line);
        void AddResultFile(string filePath);
    }
    
    public class MSTestTestContextProvider : IMSTestTestContextProvider
    {
        private readonly TestContextWrapper _testContext;
        private readonly Lazy<IContextManager> _contextManager;

        public MSTestTestContextProvider(IObjectContainer container, TestContextWrapper testContext)
        {
            _testContext = testContext;
            _contextManager = new Lazy<IContextManager>(container.Resolve<IContextManager>);
        }

        public void WriteLine(string line)
        {
            var type = GetTestContext().TestContext.GetType();
            var writeLineMethod = type.GetMethod("WriteLine", [typeof(string)]);
            writeLineMethod.Invoke(_testContext.TestContext, [line]);
        }

        public void AddResultFile(string filePath)
        {
            var type = GetTestContext().TestContext.GetType();
            var writeLineMethod = type.GetMethod("AddResultFile", [typeof(string)]);
            writeLineMethod.Invoke(_testContext.TestContext, [filePath]);
        }

        private TestContextWrapper GetTestContext()
        {
            var scenarioContext = _contextManager.Value.ScenarioContext;

            // if we're not in the context of a scenario we use the global TestContext registered in the generated AssemblyInitialize class via the MsTestContainerBuilder
            if (scenarioContext == null)
                return _testContext;

            // if we're in the context of a scenario we use the test specific TestContext instance registered in the generated test class in the ScenarioInitialize method
            var msTestType = 
                Type.GetType("Microsoft.VisualStudio.TestTools.UnitTesting.TestContext, Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions", false)
                ?? Type.GetType("Microsoft.VisualStudio.TestTools.UnitTesting.TestContext, MSTest.TestFramework.Extensions", false);
            if (msTestType == null)
                throw new InvalidOperationException("Cannot resolve TestContext from scenario");
            return new TestContextWrapper(scenarioContext.ScenarioContainer.Resolve(msTestType));
        }
    }
}
