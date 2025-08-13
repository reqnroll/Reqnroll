using System;
using System.Collections.Generic;

namespace Reqnroll.Assist
{
    #nullable enable
    public class Service
    {
        public ServiceComponentList<IValueComparer> ValueComparers { get; private set; }
        public ServiceComponentList<IValueRetriever> ValueRetrievers { get; private set; }

        public static Service Instance { get; } = new Service();

        public Service()
        {
            ValueComparers = new ReqnrollDefaultValueComparerList();
            ValueRetrievers = new ReqnrollDefaultValueRetrieverList();
        }

        public void RestoreDefaults()
        {
            ValueComparers = new ReqnrollDefaultValueComparerList();
            ValueRetrievers = new ReqnrollDefaultValueRetrieverList();
        }

        public IValueRetriever? GetValueRetrieverFor(DataTableRow row, Type targetType, Type propertyType)
        {
            var keyValuePair = new KeyValuePair<string, string>(row[0], row[1]);
            foreach (var valueRetriever in ValueRetrievers)
            {
                if (valueRetriever.CanRetrieve(keyValuePair, targetType, propertyType))
                {
                    return valueRetriever;
                }
            }
            return null;
        }

    }
}

