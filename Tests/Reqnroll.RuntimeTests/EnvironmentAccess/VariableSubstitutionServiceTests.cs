using FluentAssertions;
using Moq;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Time;
using System;
using Xunit;

namespace Reqnroll.RuntimeTests.EnvironmentAccess;

public class VariableSubstitutionServiceTests
{
    private readonly Mock<IEnvironmentWrapper> _environmentWrapperMock;
    private readonly Mock<IClock> _clockMock;
    private readonly Mock<IBuildMetadataProvider> _buildMetadataProviderMock;
    private readonly VariableSubstitutionService _sut;

    public VariableSubstitutionServiceTests()
    {
        _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
        _clockMock = new Mock<IClock>();
        _buildMetadataProviderMock = new Mock<IBuildMetadataProvider>();
        _buildMetadataProviderMock.Setup(b => b.GetBuildMetadata())
            .Returns(new BuildMetadata("http://ci.example.com/build/123", "123", "origin", "rev-456", "main", "v1.0.0"));
        _sut = new VariableSubstitutionService(_environmentWrapperMock.Object, _clockMock.Object, _buildMetadataProviderMock.Object);
    }
    // Tests for variable substitution in output file path
    [Fact]
    public void ResolveOutputFilePathVariables_NoVariables_ReturnsSamePath()
    {
        // Act
        var result = _sut.ResolveTemplatePlaceholders("results.txt");
        // Assert
        result.Should().Be("results.txt");
    }
    [Fact]
    public void ResolveOutputFilePathVariables_WithNullEmptyInput_ReturnsEmptyString()
    {
        // Act
        var resultNull = _sut.ResolveTemplatePlaceholders(null);
        var resultEmpty = _sut.ResolveTemplatePlaceholders("");
        // Assert
        resultNull.Should().BeNull();
        resultEmpty.Should().BeEmpty();
    }
    [Fact]
    public void ResolveOutputFilePathVariables_ReplacesTimestampVariable()
    {
        // Arrange
        _clockMock.Setup(c => c.GetNowDateAndTime()).Returns(new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc));
        // Act
        var result = _sut.ResolveTemplatePlaceholders("results_{timestamp}.txt");
        // Assert
        result.Should().Be("results_2023-01-02_03-04-05.txt");
    }

    [Theory]
    [InlineData("buildNumber", "123")]
    [InlineData("revision", "rev-456")]
    [InlineData("branch", "main")]
    [InlineData("tag", "v1.0.0")]
    public void ResolveOutputFilePathVariables_ReplacesBuildMetadataVariables(string variableName, string expectedValue)
    {
        // Arrange
        var input = $"results_{{{variableName}}}.txt";
        // Act
        var result = _sut.ResolveTemplatePlaceholders(input);
        // Assert
        result.Should().Be($"results_{expectedValue}.txt");
    }


    [Fact]
    public void ResolveOutputFilePathVariables_ReplacesEnvironmentVariable()
    {
        // Arrange
        _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable("MY_ENV_VAR"))
            .Returns(Result<string>.Success("envValue"));
        var input = "results_{env:MY_ENV_VAR}.txt";
        // Act
        var result = _sut.ResolveTemplatePlaceholders(input);
        // Assert
        result.Should().Be("results_envValue.txt");
    }

    [Fact]
    public void ResolveOutputFilePathVariables_UnknownVariableLeftUnchanged()
    {
        // Arrange
        var input = "results_{unknownvar}.txt";
        // Act
        var result = _sut.ResolveTemplatePlaceholders(input);
        // Assert
        result.Should().Be("results_{unknownvar}.txt");
    }

    [Fact]
    public void ResolveOutputFilePathVariables_MultipleVariables_AllSubstituted()
    {
        // Arrange
        _clockMock.Setup(c => c.GetNowDateAndTime()).Returns(new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc));
        _environmentWrapperMock.Setup(e => e.GetEnvironmentVariable("MY_ENV_VAR"))
            .Returns(Result<string>.Success("envValue"));
        var input = "results_{timestamp}_{env:MY_ENV_VAR}.txt";
        // Act
        var result = _sut.ResolveTemplatePlaceholders(input);
        // Assert
        result.Should().Be("results_2023-01-02_03-04-05_envValue.txt");
    }

    [Fact]
    public void ResolveOutputFilePathVariables_MalformedPattern_NotSubstituted()
    {
        // Arrange
        var input = "results_{timestamp.txt";
        // Act
        var result = _sut.ResolveTemplatePlaceholders(input);
        // Assert
        result.Should().Be("results_{timestamp.txt");
    }
}
