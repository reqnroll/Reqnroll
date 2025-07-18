using System;
using System.Collections.Generic;
using System.Reflection;

namespace Reqnroll.Bindings.Reflection
{
    public class BindingMethodEqualityComparer : IEqualityComparer<IBindingMethod>
    {
        public bool Equals(IBindingMethod x, IBindingMethod y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            if (x.GetType() != y.GetType())
                return false;

            // Special handling for RuntimeBindingMethod
            if (x is RuntimeBindingMethod rx && y is RuntimeBindingMethod ry)
            {
                var miX = rx.MethodInfo;
                var miY = ry.MethodInfo;
                if (miX == miY)
                    return true;
                if (miX is null || miY is null)
                    return false;
                // Consider equal if MetadataToken, Module, and DeclaringType are equal
                return miX.MetadataToken == miY.MetadataToken &&
                       Equals(miX.Module, miY.Module) &&
                       Equals(miX.DeclaringType, miY.DeclaringType);
            }

            // Fallback to default equality
            return x.Equals(y);
        }

        public int GetHashCode(IBindingMethod obj)
        {
            if (obj is null)
                return 0;
            if (obj is RuntimeBindingMethod rbm && rbm.MethodInfo != null)
            {
                // Use MetadataToken and Module for hash code
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + rbm.MethodInfo.MetadataToken.GetHashCode();
                    hash = hash * 23 + (rbm.MethodInfo.Module?.GetHashCode() ?? 0);
                    hash = hash * 23 + (rbm.MethodInfo.DeclaringType?.GetHashCode() ?? 0);
                    return hash;
                }
            }
            return obj.GetHashCode();
        }
    }
}
