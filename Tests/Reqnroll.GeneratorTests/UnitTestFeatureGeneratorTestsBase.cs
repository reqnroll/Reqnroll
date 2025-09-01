using Moq;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests;

public abstract class UnitTestFeatureGeneratorTestsBase
{
    protected UnitTestFeatureGeneratorTestsBase()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        SetupInternal();
    }

    protected Mock<IUnitTestGeneratorProvider> UnitTestGeneratorProviderMock { get; private set; }
    protected IObjectContainer Container { get; private set; }

    protected virtual void SetupInternal()
    {
        Container = new GeneratorContainerBuilder().CreateContainer(new ReqnrollConfigurationHolder(ConfigSource.Default, null), new ProjectSettings(), []);
        UnitTestGeneratorProviderMock = new Mock<IUnitTestGeneratorProvider>();
        Container.RegisterInstanceAs(UnitTestGeneratorProviderMock.Object);
    }

    protected IFeatureGenerator CreateUnitTestFeatureGenerator()
    {
        return Container.Resolve<UnitTestFeatureGeneratorProvider>().CreateGenerator(ParserHelper.CreateAnyDocument());
    }

    protected void GenerateFeature(IFeatureGenerator generator, ReqnrollDocument document)
    {
        generator.GenerateUnitTestFixture(document, "dummy", "dummyNS");
    }
}
