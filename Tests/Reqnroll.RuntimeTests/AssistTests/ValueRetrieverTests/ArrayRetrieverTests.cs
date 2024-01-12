using System;
using System.Collections.Generic;
using Reqnroll.Assist.ValueRetrievers;

namespace Reqnroll.RuntimeTests.AssistTests.ValueRetrieverTests
{
    public class ArrayRetrieverTests : EnumerableRetrieverTests
    {
        protected override EnumerableValueRetriever CreateTestee()
        {
            return new ArrayValueRetriever();
        }

        protected override IEnumerable<Type> BuildPropertyTypes(Type valueType)
        {
            yield return valueType.MakeArrayType();
        }
    }
}