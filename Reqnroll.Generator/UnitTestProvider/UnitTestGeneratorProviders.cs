using Reqnroll.BoDi;
using Reqnroll.Generator.UnitTestProvider;

namespace Reqnroll.Generator
{
    partial class DefaultDependencyProvider
    {
        partial void RegisterUnitTestGeneratorProviders(ObjectContainer container)
        {
            container.RegisterTypeAs<NUnit3TestGeneratorProvider, IUnitTestGeneratorProvider>("nunit");
            container.RegisterTypeAs<XUnit2TestGeneratorProvider, IUnitTestGeneratorProvider>("xunit");
            container.RegisterTypeAs<XUnit3TestGeneratorProvider, IUnitTestGeneratorProvider>("xunit3");
            container.RegisterTypeAs<MsTestV2GeneratorProvider, IUnitTestGeneratorProvider>("mstest");
        }
    }
}
