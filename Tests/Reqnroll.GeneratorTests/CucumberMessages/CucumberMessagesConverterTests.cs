using FluentAssertions;
using Gherkin.CucumberMessages;
using Reqnroll.GeneratorTests.Helper;
using Reqnroll.Parser;
using Reqnroll.Parser.CucumberMessages;
using Xunit;

namespace Reqnroll.GeneratorTests.CucumberMessages;

public class CucumberMessagesConverterTests
{
    [Fact]
    public void ConvertToCucumberMessagesGherkinDocument_should_combine_folder_and_file_with_slash()
    {
        // Arrange
        var sut = new CucumberMessagesConverter(new IncrementingIdGenerator());
        using var tempFile = new TempFile(".feature");
        tempFile.SetContent("Feature: Addition");
        var documentLocation = new ReqnrollDocumentLocation(
            tempFile.FullPath,
            "Features/MyFolder");
        var reqnrollDocument = ParserHelper.CreateAnyDocument(documentLocation: documentLocation);

        // Act
        var result = sut.ConvertToCucumberMessagesGherkinDocument(reqnrollDocument);

        // Assert
        result.Uri.Should().Be($"Features/MyFolder/{tempFile.FileName}");
    }

    [Fact]
    public void ConvertToCucumberMessagesSource_should_combine_folder_and_file_with_slash()
    {
        // Arrange
        using var tempFile = new TempFile(".feature");
        tempFile.SetContent("Feature: Addition");
        var documentLocation = new ReqnrollDocumentLocation(
            tempFile.FullPath,
            "Features/MyFolder");
        var reqnrollDocument = ParserHelper.CreateAnyDocument(documentLocation: documentLocation);

        // Act
        var result = CucumberMessagesConverter.ConvertToCucumberMessagesSource(reqnrollDocument);

        // Assert
        result.Uri.Should().Be($"Features/MyFolder/{tempFile.FileName}");
    }
}
