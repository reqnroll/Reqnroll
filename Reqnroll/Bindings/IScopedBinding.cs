namespace Reqnroll.Bindings
{
    public interface IScopedBinding
    {
        bool IsScoped { get; }
        BindingScope BindingScope { get; }
    }
}