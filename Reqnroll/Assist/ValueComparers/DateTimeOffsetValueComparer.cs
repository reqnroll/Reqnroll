using System;

namespace Reqnroll.Assist.ValueComparers
{
    public class DateTimeOffsetValueComparer : IValueComparer
    {
        public bool CanCompare(object actualValue)
        {
            return actualValue is DateTimeOffset;
        }

        public bool Compare(string expectedValue, object actualValue)
        {
            return DateTimeOffset.TryParse(expectedValue, out var expected) &&
                   expected == (DateTimeOffset)actualValue;
        }
    }
}
