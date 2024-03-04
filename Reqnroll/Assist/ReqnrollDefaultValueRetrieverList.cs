using System.Collections.Generic;
using Reqnroll.Assist.ValueRetrievers;

namespace Reqnroll.Assist
{
    #nullable enable
    internal sealed class ReqnrollDefaultValueRetrieverList : ServiceComponentList<IValueRetriever>
    {
        public ReqnrollDefaultValueRetrieverList(Service service)
            : base(new List<IValueRetriever> {
                // Sorted by likelihood
                new StringValueRetriever(),
                new IntValueRetriever(),
                new BoolValueRetriever(),
                new LongValueRetriever(),
                new FloatValueRetriever(),
                new DoubleValueRetriever(),
                new DateTimeValueRetriever(),
                new TimeSpanValueRetriever(),
                new GuidValueRetriever(),
                new EnumValueRetriever(),
                new ListValueRetriever(service),
                new ArrayValueRetriever(service),
                new ByteValueRetriever(),
                new SByteValueRetriever(),
                new UIntValueRetriever(),
                new ShortValueRetriever(),
                new UShortValueRetriever(),
                new ULongValueRetriever(),
                new DecimalValueRetriever(),
                new CharValueRetriever(),
                new DateTimeOffsetValueRetriever(),
                new UriValueRetriever()
            }, false)
        {
        }
    }
}