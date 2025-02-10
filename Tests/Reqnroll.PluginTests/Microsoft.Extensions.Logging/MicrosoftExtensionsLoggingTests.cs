using Microsoft.Extensions.Logging;
using Moq;
using Reqnroll.Microsoft.Extensions.DependencyInjection.Logging;
using Xunit;

namespace Reqnroll.PluginTests.Microsoft.Extensions.Logging;

public class MicrosoftExtensionsLoggingTests
{
    private readonly Mock<IReqnrollOutputHelper> _outputHelperMock = new();

    [Fact]
    public void ReqnrollLogger_LogWithLevelNone_ShouldNotCallWriteLine()
    {
        // Arrange
        var sut = new ReqnrollLogger(_outputHelperMock.Object, new LoggerExternalScopeProvider(), "TestCategory");

        // Act
        sut.Log(LogLevel.None, new EventId(1), "test", null, (s, _) => s.ToString());

        // Verify
        _outputHelperMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(LogLevel.Trace, "Trace test")]
    [InlineData(LogLevel.Debug, "Debug test")]
    [InlineData(LogLevel.Information, "Information test")]
    [InlineData(LogLevel.Warning, "Warning test")]
    [InlineData(LogLevel.Error, "Error test")]
    [InlineData(LogLevel.Critical, "Critical test")]
    public void ReqnrollLogger_WhenIncludeLogLevel_ShouldIncludeTheLogLevel(LogLevel level, string expectedMessage)
    {
        // Arrange
        var options = new ReqnrollLoggerOptions { IncludeLogLevel = true };
        var sut = new ReqnrollLogger(_outputHelperMock.Object, new LoggerExternalScopeProvider(), "TestCategory", options);

        // Act
        sut.Log(level, new EventId(1), "test", null, (s, _) => s.ToString());

        // Verify
        _outputHelperMock.Verify(o => o.WriteLine(expectedMessage), Times.Once);
        _outputHelperMock.VerifyNoOtherCalls();
    }
}