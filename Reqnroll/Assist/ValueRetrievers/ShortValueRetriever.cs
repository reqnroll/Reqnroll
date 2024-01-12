using System.Globalization;

namespace Reqnroll.Assist.ValueRetrievers
{
    public class ShortValueRetriever : StructRetriever<short>
    {
        protected override short GetNonEmptyValue(string value)
        {
            short.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out short returnValue);
            return returnValue;
        }
    }
}