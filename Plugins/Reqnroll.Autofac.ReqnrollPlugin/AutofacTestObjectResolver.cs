using System;
using Autofac;
using Reqnroll.BoDi;
using Reqnroll.Infrastructure;

namespace Reqnroll.Autofac
{
    public class AutofacTestObjectResolver : ITestObjectResolver
    {
        public object ResolveBindingInstance(Type bindingType, IObjectContainer container)
        {
            if (container.IsRegistered<IComponentContext>())
            {
                var componentContext = container.Resolve<IComponentContext>();
                return componentContext.Resolve(bindingType);
            }

            if (container.IsRegistered<ILifetimeScope>())
            {
                var lifeTimeScope = container.Resolve<ILifetimeScope>();
                return lifeTimeScope.Resolve(bindingType);
            }

            if (container.IsRegistered<IContainer>())
            {
                var lifeTimeScope = container.Resolve<IContainer>();
                return lifeTimeScope.Resolve(bindingType);
            }

            return container.Resolve(bindingType);
        }
    }
}
