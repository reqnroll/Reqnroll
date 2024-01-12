using System.Globalization;
using System.Threading.Tasks;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings
{
    public interface IStepArgumentTypeConverter
    {
        Task<object> ConvertAsync(object value, IBindingType typeToConvertTo, CultureInfo cultureInfo);
        bool CanConvert(object value, IBindingType typeToConvertTo, CultureInfo cultureInfo);
    }
}
