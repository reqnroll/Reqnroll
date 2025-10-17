using System;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;

namespace Reqnroll.MSTest.ReqnrollPlugin
{
    public interface IMSTestTestContextProvider
    {
        object GetTestContext();
    }
    
    public class MSTestTestContextProvider : IMSTestTestContextProvider
    {
        private readonly object _testContext;
        private readonly Lazy<IContextManager> _contextManager;

        public MSTestTestContextProvider(IObjectContainer container)
        {
            _testContext = container.Resolve(MsTestContainerBuilder.GetTestContextType());
            _contextManager = new Lazy<IContextManager>(container.Resolve<IContextManager>);
        }
        
        public object GetTestContext()
        {
            var scenarioContext = _contextManager.Value.ScenarioContext;
            
            // if we're not in the context of a scenario we use the global TestContext registered in the generated AssemblyInitialize class via the MsTestContainerBuilder
            if (scenarioContext == null)
                return _testContext;

            // if we're in the context of a scenario we use the test specific TestContext instance registered in the generated test class in the ScenarioInitialize method
            return scenarioContext.ScenarioContainer.Resolve(MsTestContainerBuilder.GetTestContextType());
        }
    }
}
