using System.Globalization;

namespace Reqnroll.Assist.ValueRetrievers
{
    public class UIntValueRetriever : StructRetriever<uint>
    {
        protected override uint GetNonEmptyValue(string value)
        {
            uint.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out uint returnValue);
            return returnValue;
        }
    }
}