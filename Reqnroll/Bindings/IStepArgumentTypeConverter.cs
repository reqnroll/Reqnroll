using System.Globalization;
using System.Threading.Tasks;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Infrastructure;

namespace Reqnroll.Bindings
{
    public interface IStepArgumentTypeConverter
    {
        Task<object> ConvertAsync(object value, IBindingType typeToConvertTo, IContextManager contextManager, CultureInfo cultureInfo);
        bool CanConvert(object value, IBindingType typeToConvertTo, CultureInfo cultureInfo);
    }
}
