using System;
using System.Collections.Generic;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection.Internal;

internal interface IAssemblyTypeResolver
{
    IEnumerable<Type> GetAssemblyTypes();
}
