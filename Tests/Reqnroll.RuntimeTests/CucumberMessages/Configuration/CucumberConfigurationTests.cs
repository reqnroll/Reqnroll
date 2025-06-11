using Moq;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Reqnroll.RuntimeTests.CucumberMessages.Configuration
{
    public class CucumberConfigurationTests
    {
        private readonly Mock<IEnvVariableEnableFlagParser> _envVariableEnableFlagParserMock;
        private readonly Mock<IFormattersConfigurationResolver> _fileResolverMock;
        private readonly Mock<IFormattersEnvironmentOverrideConfigurationResolver> _environmentResolverMock;
        private readonly FormattersConfiguration _sut;

        public CucumberConfigurationTests()
        {
            _envVariableEnableFlagParserMock = new Mock<IEnvVariableEnableFlagParser>();
            _fileResolverMock = new Mock<IFormattersConfigurationResolver>();
            _environmentResolverMock = new Mock<IFormattersEnvironmentOverrideConfigurationResolver>();

            var resolvers = new Dictionary<string, IFormattersConfigurationResolver>
            {
                {"fileBasedResolver",  _fileResolverMock.Object }
            };

            _sut = new FormattersConfiguration(resolvers, _environmentResolverMock.Object, _envVariableEnableFlagParserMock.Object);
        }

        [Fact]
        public void Enabled_Should_Return_False_When_No_Configuration_Is_Resolved()
        {
            // Arrange
            _fileResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, string>());
            _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, string>());
            _envVariableEnableFlagParserMock.Setup(p => p.Parse()).Returns(true);

            // Act
            var result = _sut.Enabled;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Enabled_Should_Respect_Environment_Variable_Override()
        {
            // Arrange
            var mockedSetup = new Dictionary<string, string>();
            mockedSetup.Add("html", @"c:\html\html_report.html");
            _fileResolverMock.Setup(r => r.Resolve()).Returns(mockedSetup);
            _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, string>());

            _envVariableEnableFlagParserMock.Setup(p => p.Parse()).Returns(false);

            // Act
            var result = _sut.Enabled;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFormatterConfigurationByName_Should_Return_Configuration_For_Existing_Formatter()
        {
            // Arrange
            var mockedSetup = new Dictionary<string, string>();
            mockedSetup.Add("html", @"c:\html\html_report.html");
            _fileResolverMock.Setup(r => r.Resolve()).Returns(mockedSetup);
            _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, string>());
            _envVariableEnableFlagParserMock.Setup(p => p.Parse()).Returns(true);

            // Act
            var result = _sut.GetFormatterConfigurationByName("html");

            // Assert
            Assert.Equal(@"c:\html\html_report.html", result);
        }

        [Fact]
        public void GetFormatterConfigurationByName_Should_Respect_Formatter_Given_By_EnvironmentVariable_Override()
        {
            // Arrange
            var configFileSetup = new Dictionary<string, string>();
            configFileSetup.Add("html", @"c:\html\html_report.html");
            _fileResolverMock.Setup(r => r.Resolve()).Returns(configFileSetup);

            var envVarSetup = new Dictionary<string, string>();
            envVarSetup.Add("html", @"c:\html\html_overridden_name.html");
            _environmentResolverMock.Setup(r => r.Resolve()).Returns(envVarSetup);
            _envVariableEnableFlagParserMock.Setup(p => p.Parse()).Returns(true);

            // Act
            var result = _sut.GetFormatterConfigurationByName("html");

            // Assert
            Assert.Equal(@"c:\html\html_overridden_name.html", result);
        }


        [Fact]
        public void GetFormatterConfigurationByName_Should_Return_Empty_For_Nonexistent_Formatter()
        {
            // Arrange
            _fileResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, string>());
            _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, string>());

            _envVariableEnableFlagParserMock.Setup(p => p.Parse()).Returns(true);

            // Act
            var result = _sut.GetFormatterConfigurationByName("nonexistent");

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void SetEnabled_Should_Override_Runtime_Enablement()
        {
            // Arrange
            var mockedSetup = new Dictionary<string, string>();
            mockedSetup.Add("html", @"c:\html\html_report.html");
            _fileResolverMock.Setup(r => r.Resolve()).Returns(mockedSetup);
            _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, string>());
            _envVariableEnableFlagParserMock.Setup(p => p.Parse()).Returns(true);

            // Act
            _sut.SetEnabled(false);
            var result = _sut.Enabled;

            // Assert
            Assert.False(result);
        }
    }
}
