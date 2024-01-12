using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Assist.ValueRetrievers;

namespace Reqnroll.RuntimeTests.AssistTests.ValueRetrieverTests
{
    public class ListRetrieverTests : EnumerableRetrieverTests
    {
        protected override EnumerableValueRetriever CreateTestee()
        {
            return new ListValueRetriever();
        }

        protected override IEnumerable<Type> BuildPropertyTypes(Type valueType)
        {
            var propertyTypeDefinitions = new[]
            {
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(IList<>),
                typeof(List<>),
                typeof(IReadOnlyList<>),
                typeof(IReadOnlyCollection<>),
            };
            
            return propertyTypeDefinitions.Select(x => x.MakeGenericType(valueType));
        }
    }
}
