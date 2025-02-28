namespace Reqnroll.Bindings.Reflection
{
    public interface IBindingParameter
    {
        IBindingType Type { get; }
        string ParameterName { get; }

        bool IsOptional { get; }

        object OptionalValue { get; }
    }
}