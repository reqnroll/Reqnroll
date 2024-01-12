namespace Reqnroll.Bindings.Reflection
{
    public interface IPolymorphicBindingType : IBindingType
    {
        bool IsAssignableTo(IBindingType baseType);
    }
}