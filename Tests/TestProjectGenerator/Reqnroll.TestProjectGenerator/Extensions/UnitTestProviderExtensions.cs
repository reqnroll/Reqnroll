using System;

namespace Reqnroll.TestProjectGenerator.Extensions
{
    public static class UnitTestProviderExtensions
    {
        public static string ToName(this UnitTestProvider unitTestProvider)
        {
            switch (unitTestProvider)
            {
                case UnitTestProvider.MSTest: return "MSTest";
                case UnitTestProvider.NUnit2: return "NUnit2";
                case UnitTestProvider.NUnit3: return "NUnit";
                case UnitTestProvider.xUnit: return "XUnit";
                default: throw new ArgumentOutOfRangeException(nameof(unitTestProvider), unitTestProvider, "value is not known");
            }
        }
    }
}
