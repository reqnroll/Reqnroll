using System.Reflection;

namespace Reqnroll.Bindings.Discovery
{
    public interface IRuntimeBindingRegistryBuilder
    {
        void BuildBindingsFromAssembly(Assembly assembly);
        void BuildingCompleted();
    }
}