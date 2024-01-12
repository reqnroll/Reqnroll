using System;
using System.Globalization;

namespace Reqnroll.Assist.ValueRetrievers
{
    public class DecimalValueRetriever : StructRetriever<decimal>
    {
        protected override decimal GetNonEmptyValue(string value)
        {
            Decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out Decimal returnValue);
            return returnValue;
        }
    }
}