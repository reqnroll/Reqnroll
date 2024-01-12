using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings.Discovery
{
    public class BindingSourceMethod
    {
        public IBindingMethod BindingMethod { get; set; }
        public bool IsPublic { get; set; }
        public bool IsStatic { get; set; }

        public BindingSourceAttribute[] Attributes { get; set; }

        public override string ToString() => BindingMethod.ToString();
    }
}