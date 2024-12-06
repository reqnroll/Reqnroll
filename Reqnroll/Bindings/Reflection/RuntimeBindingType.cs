using System;
using System.Linq;

namespace Reqnroll.Bindings.Reflection
{
    public class RuntimeBindingType : IPolymorphicBindingType
    {
        public readonly Type Type;

        public string Name => Type.Name;

        public string FullName { get; }

        public string AssemblyName => Type.Assembly.GetName().Name;

        public RuntimeBindingType(Type type)
        {
            Type = type;
            FullName = GetFullName(type);
        }

        private static string GetFullName(Type type)
        {
            if (!type.IsConstructedGenericType)
            {
                return type.FullName;
            }

            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GenericTypeArguments[0].FullName + "?";
            }

            var genericParams = string.Join(",", type.GenericTypeArguments.Select(x => x.Name));
            return $"{type.Namespace}.{type.Name.Split('`')[0]}<{genericParams}>";
        }

        public bool IsAssignableTo(IBindingType baseType)
        {
            return Type.IsAssignableTo(baseType);
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        protected bool Equals(RuntimeBindingType other)
        {
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RuntimeBindingType)obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }

        public static readonly RuntimeBindingType Void = new(typeof(void));
    }
}