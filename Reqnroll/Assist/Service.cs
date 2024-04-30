using System;
using System.Collections.Generic;

namespace Reqnroll.Assist
{
    #nullable enable
    public class Service
    {
        public ServiceComponentList<IValueComparer> ValueComparers { get; private set; }
        public ServiceComponentList<IValueRetriever> ValueRetrievers { get; private set; }

        [Obsolete("Use TableHelpers retrieved from the DI container instead.")]
        public static Service Instance { get; } = new Service();

        public Service()
        {
            ValueComparers = new ReqnrollDefaultValueComparerList();
            ValueRetrievers = new ReqnrollDefaultValueRetrieverList(this);
        }

        public void RestoreDefaults()
        {
            ValueComparers = new ReqnrollDefaultValueComparerList();
            ValueRetrievers = new ReqnrollDefaultValueRetrieverList(this);
        }

        [Obsolete("Use ValueComparers.Register")]
        public void RegisterValueComparer(IValueComparer valueComparer)
        {
            ValueComparers.Register(valueComparer);
        }

        [Obsolete("Use ValueComparers.Unregister")]
        public void UnregisterValueComparer(IValueComparer valueComparer)
        {
            ValueComparers.Unregister(valueComparer);
        }

        [Obsolete("Use ValueRetrievers.Register")]
        public void RegisterValueRetriever(IValueRetriever valueRetriever)
        {
            ValueRetrievers.Register(valueRetriever);
        }

        [Obsolete("Use ValueRetrievers.Unregister")]
        public void UnregisterValueRetriever(IValueRetriever valueRetriever)
        {
            ValueRetrievers.Unregister(valueRetriever);
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

