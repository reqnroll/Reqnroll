using System;
using System.Collections.Generic;
using Reqnroll.Assist;
using Reqnroll.Assist.ValueRetrievers;

namespace Reqnroll.RuntimeTests.AssistTests.ValueRetrieverTests
{
    public class ArrayRetrieverTests : EnumerableRetrieverTests
    {
        protected override EnumerableValueRetriever CreateTestee(Service service)
        {
            return new ArrayValueRetriever(service);
        }

        protected override IEnumerable<Type> BuildPropertyTypes(Type valueType)
        {
            yield return valueType.MakeArrayType();
        }
    }
}
