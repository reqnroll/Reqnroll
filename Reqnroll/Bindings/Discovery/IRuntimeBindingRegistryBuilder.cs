using System.Reflection;

namespace Reqnroll.Bindings.Discovery
{
    public interface IRuntimeBindingRegistryBuilder
    {
        Assembly[] GetBindingAssemblies(Assembly testAssembly);
        void BuildBindingsFromAssembly(Assembly assembly);
        void BuildingCompleted();
    }
}