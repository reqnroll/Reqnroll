using System;
using Reqnroll.BoDi;
using Moq;
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

        public Mock<IUnitTestRuntimeProvider> GetUnitTestRuntimeProviderMock()
        {
            return new Mock<IUnitTestRuntimeProvider>();
        }

        protected override void RegisterDefaults(ObjectContainer container)
        {
            base.RegisterDefaults(container);
            container.RegisterInstanceAs(GetUnitTestRuntimeProviderMock().Object, "nunit");
        }
    }
}
