using System.Collections.Generic;
using Reqnroll.Assist.ValueComparers;

namespace Reqnroll.Assist
{
    #nullable enable
    internal sealed class ReqnrollDefaultValueComparerList : ServiceComponentList<IValueComparer>
    {
        public ReqnrollDefaultValueComparerList()
            : base(new List<IValueComparer> {
                    new DateTimeValueComparer(),
                    new DateTimeOffsetValueComparer(),
                    new BoolValueComparer(),
                    new GuidValueComparer(),
                    new DecimalValueComparer(),
                    new DoubleValueComparer(),
                    new FloatValueComparer(),
                    new DefaultValueComparer(),
                }, true)
        {
        }
    }
}
