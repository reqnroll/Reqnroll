using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Assist.ValueRetrievers;
using Reqnroll.BoDi;

namespace Reqnroll.Assist
{
    #nullable enable
    public class Service
    {
        private readonly IObjectContainer? _container;

        public ServiceComponentList<IValueComparer> ValueComparers { get; private set; }
        public ServiceComponentList<IValueRetriever> ValueRetrievers { get; private set; }

        public static Service Instance { get; } = new Service();

        public Service() : this(null)
        {
        }

        public Service(IObjectContainer? container)
        {
            _container = container;
            ValueComparers = new ReqnrollDefaultValueComparerList();
            ValueRetrievers = new ReqnrollDefaultValueRetrieverList(this);

            if (!ReferenceEquals(this, Instance) && Instance != null)
            {
                CopyFrom(Instance);
            }
        }

        public void CopyFrom(Service source)
        {
            ValueComparers.Clear();
            foreach (var comparer in source.ValueComparers.Reverse())
            {
                ValueComparers.Register(comparer);
            }

            ValueRetrievers.Clear();
            foreach (var retriever in source.ValueRetrievers.Reverse())
            {
                if (retriever is ListValueRetriever)
                {
                    ValueRetrievers.Register(new ListValueRetriever(this));
                }
                else if (retriever is ArrayValueRetriever)
                {
                    ValueRetrievers.Register(new ArrayValueRetriever(this));
                }
                else if (retriever is EnumerableValueRetriever)
                {
                    var ctor = retriever.GetType().GetConstructor(new[] { typeof(Service) });
                    if (ctor != null)
                    {
                        ValueRetrievers.Register((IValueRetriever)ctor.Invoke(new object[] { this }));
                    }
                    else
                    {
                        ValueRetrievers.Register(retriever);
                    }
                }
                else
                {
                    ValueRetrievers.Register(retriever);
                }
            }
        }

        public void RestoreDefaults()
        {
            ValueComparers = new ReqnrollDefaultValueComparerList();
            ValueRetrievers = new ReqnrollDefaultValueRetrieverList(this);
        }

        public IValueRetriever? GetValueRetrieverFor(KeyValuePair<string, string> keyValuePair, Type targetType, Type propertyType)
        {
            if (_container != null)
            {
                foreach (var containerRetriever in _container.ResolveAll<IValueRetriever>())
                {
                    if (containerRetriever.CanRetrieve(keyValuePair, targetType, propertyType))
                    {
                        return containerRetriever;
                    }
                }
            }

            foreach (var valueRetriever in ValueRetrievers)
            {
                if (valueRetriever.CanRetrieve(keyValuePair, targetType, propertyType))
                {
                    return valueRetriever;
                }
            }
            return null;
        }

        public IValueRetriever? GetValueRetrieverFor(DataTableRow row, Type targetType, Type propertyType)
        {
            return GetValueRetrieverFor(new KeyValuePair<string, string>(row[0], row[1]), targetType, propertyType);
        }

        public IValueComparer? GetValueComparerFor(object propertyValue)
        {
            if (_container != null)
            {
                foreach (var containerComparer in _container.ResolveAll<IValueComparer>())
                {
                    if (containerComparer.CanCompare(propertyValue))
                    {
                        return containerComparer;
                    }
                }
            }

            return ValueComparers.FirstOrDefault(x => x.CanCompare(propertyValue));
        }
    }
}
