using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.Extensions
{
    public static class UnitTestProviderExtensions
    {
        public static string ToName(this UnitTestProvider unitTestProvider)
        {
            switch (unitTestProvider)
            {
                case UnitTestProvider.SpecRunWithNUnit: return "SpecRun+NUnit";
                case UnitTestProvider.SpecRunWithNUnit2: return "SpecRun+NUnit.2";
                case UnitTestProvider.SpecRunWithMsTest: return "SpecRun+MsTest";
                case UnitTestProvider.MSTest: return "MSTest";
                case UnitTestProvider.NUnit2: return "NUnit2";
                case UnitTestProvider.NUnit3: return "NUnit";
                case UnitTestProvider.xUnit: return "XUnit";
                case UnitTestProvider.SpecRun: return "SpecRun";
                default: throw new ArgumentOutOfRangeException(nameof(unitTestProvider), unitTestProvider, "value is not known");
            }
        }
    }
}
