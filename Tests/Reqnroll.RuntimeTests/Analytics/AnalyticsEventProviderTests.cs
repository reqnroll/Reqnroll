using FluentAssertions;
using NSubstitute;
using Reqnroll.Analytics;
using Reqnroll.Analytics.UserId;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Xunit;

namespace Reqnroll.RuntimeTests.Analytics
{
    public class AnalyticsEventProviderTests
    {
        [Fact]
        public void Should_return_the_build_server_name_in_Compiling_Event()
        {
            var userUniqueIdStoreMock = Substitute.For<IUserUniqueIdStore>();
            var environmentMock = Substitute.For<IEnvironmentWrapper>();
            var sut = new AnalyticsEventProvider(userUniqueIdStoreMock, new UnitTestProvider.UnitTestProviderConfiguration(), environmentMock);

            environmentMock
                .GetEnvironmentVariable("TF_BUILD")
                .Returns(new Success<string>("true"));

            var compilingEvent = sut.CreateProjectCompilingEvent(null, null, null, null, null);
            
            compilingEvent.BuildServerName.Should().Be("Azure Pipelines");
        }

        [Fact]
        public void Should_return_the_build_server_name_in_Running_Event()
        {
            var userUniqueIdStoreMock = Substitute.For<IUserUniqueIdStore>();
            var environmentMock = Substitute.For<IEnvironmentWrapper>();
            var sut = new AnalyticsEventProvider(userUniqueIdStoreMock, new UnitTestProvider.UnitTestProviderConfiguration(), environmentMock);

            environmentMock
                .GetEnvironmentVariable("TEAMCITY_VERSION")
                .Returns(new Success<string>("true"));

            var compilingEvent = sut.CreateProjectRunningEvent(null);
            
            compilingEvent.BuildServerName.Should().Be("TeamCity");
        }

        [Fact]
        public void Should_return_null_for_the_build_server_name_when_not_detected()
        {
            var userUniqueIdStoreMock = Substitute.For<IUserUniqueIdStore>();
            var environmentMock = Substitute.For<IEnvironmentWrapper>();
            var sut = new AnalyticsEventProvider(userUniqueIdStoreMock, new UnitTestProvider.UnitTestProviderConfiguration(), environmentMock);

            var compilingEvent = sut.CreateProjectRunningEvent(null);
            
            compilingEvent.BuildServerName.Should().Be(null);
        }
    }
}
