using System;
using Reqnroll.BoDi;
using Reqnroll.UnitTestProvider;
using Xunit;

namespace Reqnroll.GeneratorTests
{
    public class UnitTestProviderConfigurationTests
    {
        private readonly IObjectContainer container;

        public UnitTestProviderConfigurationTests()
        {
            container = new ObjectContainer();
        }

        [Fact]
        public void ShouldGetAnExceptionWhenSettingUnitTestProviderToNull()
        {
            var unitTestProvider = container.Resolve<UnitTestProviderConfiguration>();
            Assert.Throws<ArgumentNullException>(() => unitTestProvider.UseUnitTestProvider(null));
        }

        [Fact]
        public void ShouldGetAnExceptionWhenUnitTestProviderIsAlreadySet()
        {
            var unitTestProvider = container.Resolve<UnitTestProviderConfiguration>();
            unitTestProvider.UseUnitTestProvider("nunit");
            Assert.Throws<Exception>(() => unitTestProvider.UseUnitTestProvider("xunit"));
        }
    }
}
