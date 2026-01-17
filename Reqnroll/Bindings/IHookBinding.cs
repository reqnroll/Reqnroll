namespace Reqnroll.Bindings
{
    public interface IHookBinding : IScopedBinding, IBinding
    {
        HookType HookType { get; }
        int HookOrder { get; }
        bool IsValid { get; }
        string ErrorMessage { get; }
    }
}