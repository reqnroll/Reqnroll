using System;
using Reqnroll.BoDi;
using Castle.Windsor;
using Reqnroll.Infrastructure;

namespace Reqnroll.Windsor
{
    public class WindsorTestObjectResolver : ITestObjectResolver
    {
        public object ResolveBindingInstance(Type bindingType, IObjectContainer scenarioContainer)
        {
            if (scenarioContainer.IsRegistered<IWindsorContainer>())
            {
                var componentContext = scenarioContainer.Resolve<IWindsorContainer>();
                return componentContext.Resolve(bindingType);
            }
            return scenarioContainer.Resolve(bindingType);
        }
    }
}
