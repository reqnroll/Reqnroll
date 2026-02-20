using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.PubSub;

public class CucumberMessageFactoryTests : IDisposable
{
    private readonly CucumberMessageFactory _sut = new();
    private readonly List<string> _tempFiles = new();

    private string CreateTempFile(string extension, string content, Encoding encoding)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}{extension}");
        File.WriteAllText(path, content, encoding);
        _tempFiles.Add(path);
        return path;
    }

    private string CreateTempBinaryFile(string extension, byte[] content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}{extension}");
        File.WriteAllBytes(path, content);
        _tempFiles.Add(path);
        return path;
    }

    private AttachmentTracker CreateTracker(string filePath)
    {
        // Use reflection to construct AttachmentTracker (internal constructor)
        var factory = new CucumberMessageFactory();
        var publisher = new Mock<Reqnroll.Formatters.PubSub.IMessagePublisher>().Object;
        return (AttachmentTracker)Activator.CreateInstance(
            typeof(AttachmentTracker),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new object[] { "runId", "caseId", "stepId", "hookId", factory, publisher },
            null)!;
    }

    // --- text/* MIME types ---

    [Theory]
    [InlineData(".txt", "text/plain")]
    [InlineData(".csv", "text/csv")]
    [InlineData(".html", "text/html")]
    [InlineData(".htm", "text/html")]
    [InlineData(".css", "text/css")]
    [InlineData(".ics", "text/calendar")]
    public void ToAttachment_TextFile_WithNonAsciiContent_Base64DecodesCorrectly(string extension, string expectedMimeType)
    {
        const string nonAsciiContent = "Héllo Wörld – こんにちは";
        var filePath = CreateTempFile(extension, nonAsciiContent, Encoding.UTF8);
        var tracker = CreateTracker(filePath);
        tracker.GetType().GetProperty("FilePath")!.SetValue(tracker, filePath);
        tracker.GetType().GetProperty("Timestamp")!.SetValue(tracker, DateTime.UtcNow);

        var attachment = _sut.ToAttachment(tracker);

        attachment.ContentEncoding.Should().Be(AttachmentContentEncoding.BASE64);
        attachment.MediaType.Should().Be(expectedMimeType);

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(attachment.Body));
        decoded.Should().Be(nonAsciiContent);
    }

    // --- application/* text-based MIME types ---

    [Theory]
    [InlineData(".json", "application/json")]
    [InlineData(".xml", "application/xml")]
    [InlineData(".js", "text/javascript")]
    public void ToAttachment_TextBasedApplicationFile_WithNonAsciiContent_Base64DecodesCorrectly(string extension, string expectedMimeType)
    {
        const string nonAsciiContent = "{ \"name\": \"Ünïcödé\" }";
        var filePath = CreateTempFile(extension, nonAsciiContent, Encoding.UTF8);
        var tracker = CreateTracker(filePath);
        tracker.GetType().GetProperty("FilePath")!.SetValue(tracker, filePath);
        tracker.GetType().GetProperty("Timestamp")!.SetValue(tracker, DateTime.UtcNow);

        var attachment = _sut.ToAttachment(tracker);

        attachment.ContentEncoding.Should().Be(AttachmentContentEncoding.BASE64);
        attachment.MediaType.Should().Be(expectedMimeType);

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(attachment.Body));
        decoded.Should().Be(nonAsciiContent);
    }

    // --- ASCII-only text: regression guard ---

    [Fact]
    public void ToAttachment_TextFile_WithAsciiOnlyContent_Base64DecodesCorrectly()
    {
        const string asciiContent = "Hello World";
        var filePath = CreateTempFile(".txt", asciiContent, Encoding.UTF8);
        var tracker = CreateTracker(filePath);
        tracker.GetType().GetProperty("FilePath")!.SetValue(tracker, filePath);
        tracker.GetType().GetProperty("Timestamp")!.SetValue(tracker, DateTime.UtcNow);

        var attachment = _sut.ToAttachment(tracker);

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(attachment.Body));
        decoded.Should().Be(asciiContent);
    }

    // --- Binary files: untouched byte-for-byte ---

    [Theory]
    [InlineData(".png", "image/png")]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".bin", "application/octet-stream")]
    public void ToAttachment_BinaryFile_Base64MatchesRawBytes(string extension, string expectedMimeType)
    {
        var binaryContent = new byte[] { 0xFF, 0xD8, 0xFE, 0x00, 0x01, 0x80 };
        var filePath = CreateTempBinaryFile(extension, binaryContent);
        var tracker = CreateTracker(filePath);
        tracker.GetType().GetProperty("FilePath")!.SetValue(tracker, filePath);
        tracker.GetType().GetProperty("Timestamp")!.SetValue(tracker, DateTime.UtcNow);

        var attachment = _sut.ToAttachment(tracker);

        attachment.ContentEncoding.Should().Be(AttachmentContentEncoding.BASE64);
        attachment.MediaType.Should().Be(expectedMimeType);

        var decoded = Convert.FromBase64String(attachment.Body);
        decoded.Should().Equal(binaryContent);
    }

    // --- Unknown extension: falls back to binary handling ---

    [Fact]
    public void ToAttachment_UnknownExtension_FallsBackToBinaryHandling()
    {
        var binaryContent = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        var filePath = CreateTempBinaryFile(".xyz_unknown", binaryContent);
        var tracker = CreateTracker(filePath);
        tracker.GetType().GetProperty("FilePath")!.SetValue(tracker, filePath);
        tracker.GetType().GetProperty("Timestamp")!.SetValue(tracker, DateTime.UtcNow);

        var attachment = _sut.ToAttachment(tracker);

        attachment.ContentEncoding.Should().Be(AttachmentContentEncoding.BASE64);
        attachment.MediaType.Should().Be("application/octet-stream");

        var decoded = Convert.FromBase64String(attachment.Body);
        decoded.Should().Equal(binaryContent);
    }

    // --- Metadata correctness ---

    [Fact]
    public void ToAttachment_SetsCorrectFileName()
    {
        var filePath = CreateTempFile(".txt", "test", Encoding.UTF8);
        var tracker = CreateTracker(filePath);
        tracker.GetType().GetProperty("FilePath")!.SetValue(tracker, filePath);
        tracker.GetType().GetProperty("Timestamp")!.SetValue(tracker, DateTime.UtcNow);

        var attachment = _sut.ToAttachment(tracker);

        attachment.FileName.Should().Be(Path.GetFileName(filePath));
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
            if (File.Exists(file)) File.Delete(file);
    }
}