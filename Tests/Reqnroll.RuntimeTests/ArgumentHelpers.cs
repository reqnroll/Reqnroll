using Moq;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.RuntimeTests;

internal static class ArgumentHelpers
{
    public static IBindingType IsBindingType<T>()
    {
        return It.Is<IBindingType>(bt => bt.TypeEquals(typeof(T)));
    }
}
