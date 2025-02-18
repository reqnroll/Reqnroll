using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Bindings;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection.Internal;

internal class AssemblyTypeResolver(IBindingRegistry bindingRegistry) : IAssemblyTypeResolver
{
    public IEnumerable<Type> GetAssemblyTypes() => bindingRegistry.GetBindingAssemblies().SelectMany(a => a.GetTypes());
}
