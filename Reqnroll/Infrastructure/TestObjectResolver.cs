using System;
using Reqnroll.BoDi;

namespace Reqnroll.Infrastructure
{
    public class TestObjectResolver : ITestObjectResolver
    {
        public object ResolveBindingInstance(Type bindingType, IObjectContainer container)
        {
            return container.Resolve(bindingType);
        }
    }
}
