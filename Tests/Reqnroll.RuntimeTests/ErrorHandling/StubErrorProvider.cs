using System;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Reqnroll.Configuration;
using Reqnroll.ErrorHandling;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.RuntimeTests.ErrorHandling
{
    internal class StubErrorProvider : ErrorProvider
    {
        public StubErrorProvider() :
            base(new StepFormatter(new ColorOutputHelper(ConfigurationLoader.GetDefault()), new ColorOutputTheme()), ConfigurationLoader.GetDefault(), GetStubUnitTestProvider())
        {
        }

        private static IUnitTestRuntimeProvider GetStubUnitTestProvider()
        { 
            //TODO NSub check
            var mock = Substitute.For<IUnitTestRuntimeProvider>();
            mock.When(m => m.TestIgnore(Arg.Any<string>())).Throws<InvalidOperationException>();
            mock.When(m => m.TestInconclusive(Arg.Any<string>())).Throws<InvalidOperationException>();
            mock.When(m => m.TestPending(Arg.Any<string>())).Throws<InvalidOperationException>();
            return mock;
        }
    }
}
