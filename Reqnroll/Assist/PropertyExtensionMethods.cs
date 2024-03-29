using System.Linq;
using System.Reflection;

namespace Reqnroll.Assist
{
    internal static class PropertyExtensionMethods
    {
        public static object GetPropertyValue(this object @object, string propertyName)
        {
            var property = GetThePropertyOnThisObject(@object, propertyName);
            return property.GetValue(@object, null);
        }

        public static void SetPropertyValue(this object @object, string propertyName, object value)
        {
            var property = GetThePropertyOnThisObject(@object, propertyName);
            property.SetValue(@object, value, null);
        }

        public static PropertyInfo GetThePropertyOnThisObject(this object @object, string propertyName)
        {
            var type = @object.GetType();
            return type.GetProperties()
                .FirstOrDefault(x => TEHelpers.IsMemberMatchingToColumnName(x, propertyName));
        }
    }
}
