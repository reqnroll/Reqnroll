using System.Collections.Generic;
using System.Reflection;

namespace Reqnroll.Autofac;

public interface IConfigurationMethodsProvider
{
    IEnumerable<MethodInfo> GetConfigurationMethods();
}
