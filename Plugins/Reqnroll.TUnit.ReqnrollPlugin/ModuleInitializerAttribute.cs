#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class ModuleInitializerAttribute : Attribute { }
#endif
