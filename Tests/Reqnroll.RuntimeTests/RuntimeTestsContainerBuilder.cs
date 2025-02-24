using Reqnroll.BoDi;
using NSubstitute;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.RuntimeTests
{
    public class RuntimeTestsContainerBuilder : ContainerBuilder
    {
        public RuntimeTestsContainerBuilder(IDefaultDependencyProvider defaultDependencyProvider = null)
            : base(defaultDependencyProvider)
        {
            
        }

        public IUnitTestRuntimeProvider GetUnitTestRuntimeProviderMock()
        {
            return Substitute.For<IUnitTestRuntimeProvider>();
        }

        protected override void RegisterDefaults(ObjectContainer container)
        {
            base.RegisterDefaults(container);
            container.RegisterInstanceAs(GetUnitTestRuntimeProviderMock(), "nunit");
        }
    }
}
