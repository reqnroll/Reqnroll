using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using Moq;
using Reqnroll.Tools.MsBuild.Generation;
using Xunit;

namespace Reqnroll.GeneratorTests.MSBuildTask;

/// <summary>
/// Tests for the extended-length path handling of <see cref="GeneratedFileWriter"/>,
/// see https://github.com/reqnroll/Reqnroll/issues/1128
/// </summary>
public class GeneratedFileWriterTests
{
    private readonly Mock<IReqnrollTaskLoggingHelper> _logMock = new();
    private int _supportCheckCount;

    private GeneratedFileWriter CreateSut(bool isExtendedLengthPathSupported = true, bool isWindows = true)
    {
        return new GeneratedFileWriter(_logMock.Object, () =>
        {
            _supportCheckCount++;
            return isExtendedLengthPathSupported;
        }, isWindows);
    }

    private static string GetPathWithLength(int length)
    {
        var basePath = Path.Combine(Path.GetTempPath(), "rr-long-path-test");
        var path = basePath + Path.DirectorySeparatorChar + new string('x', length - basePath.Length - 1);
        Path.GetFullPath(path).Length.Should().Be(length);
        return path;
    }

    [Fact]
    public void GetFileSystemPath_should_return_short_path_unchanged_without_checking_support()
    {
        var sut = CreateSut();
        var path = GetPathWithLength(GeneratedFileWriter.LongPathThreshold - 1);

        var result = sut.GetFileSystemPath(path);

        result.Should().Be(Path.GetFullPath(path));
        _supportCheckCount.Should().Be(0);
    }

    [Fact]
    public void GetFileSystemPath_should_return_full_path_for_relative_short_path()
    {
        var sut = CreateSut();

        var result = sut.GetFileSystemPath("SomeFolder");

        result.Should().Be(Path.GetFullPath("SomeFolder"));
    }

    [SkippableFact]
    public void GetFileSystemPath_should_use_extended_length_syntax_for_long_path_when_supported()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        var sut = CreateSut(isExtendedLengthPathSupported: true);
        var path = GetPathWithLength(GeneratedFileWriter.LongPathThreshold);

        var result = sut.GetFileSystemPath(path);

        result.Should().Be(@"\\?\" + Path.GetFullPath(path));
        _supportCheckCount.Should().Be(1);
    }

    [SkippableFact]
    public void GetFileSystemPath_should_return_long_path_unchanged_when_extended_length_syntax_is_not_supported()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        var sut = CreateSut(isExtendedLengthPathSupported: false);
        var path = GetPathWithLength(GeneratedFileWriter.LongPathThreshold);

        var result = sut.GetFileSystemPath(path);

        result.Should().Be(Path.GetFullPath(path));
        _logMock.Verify(l => l.LogTaskDiagnosticMessage(It.Is<string>(m => m.Contains("cannot be converted to extended-length path syntax"))), Times.Once);
    }

    [SkippableFact]
    public void GetFileSystemPath_should_use_extended_length_UNC_syntax_for_long_UNC_path()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        var sut = CreateSut();
        var path = @"\\server\share\" + new string('x', GeneratedFileWriter.LongPathThreshold);

        var result = sut.GetFileSystemPath(path);

        result.Should().Be(@"\\?\UNC\server\share\" + new string('x', GeneratedFileWriter.LongPathThreshold));
    }

    [SkippableFact]
    public void GetFileSystemPath_should_return_extended_length_path_unchanged()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        var sut = CreateSut();
        var path = @"\\?\" + GetPathWithLength(GeneratedFileWriter.LongPathThreshold);

        var result = sut.GetFileSystemPath(path);

        result.Should().Be(path);
        _supportCheckCount.Should().Be(0);
    }

    [Fact]
    public void GetFileSystemPath_should_not_use_extended_length_syntax_on_non_Windows_platforms()
    {
        var sut = CreateSut(isWindows: false);
        var path = GetPathWithLength(GeneratedFileWriter.LongPathThreshold + 50);

        var result = sut.GetFileSystemPath(path);

        result.Should().Be(Path.GetFullPath(path));
        _supportCheckCount.Should().Be(0);
    }

    [Fact]
    public void ExtendedLengthPathSupport_should_be_supported_on_modern_dotnet()
    {
        // .NET (Core) has no legacy path handling, so extended-length paths are always supported.
        // The legacy .NET Framework behavior cannot be tested in this test process.
        ExtendedLengthPathSupport.IsSupported.Should().BeTrue();
    }

    [Fact]
    public void WriteGeneratedFile_and_DeleteGeneratedFile_should_handle_long_paths()
    {
        var sut = new GeneratedFileWriter(_logMock.Object);
        var rootFolder = Path.Combine(Path.GetTempPath(), "rr-long-path-test", Guid.NewGuid().ToString("N"));
        var longFolder = Path.Combine(new[] { rootFolder }.Concat(Enumerable.Range(1, 6).Select(i => new string((char)('a' + i), 50))).ToArray());
        var filePath = Path.Combine(longFolder, "Sample.feature.ndjson");
        filePath.Length.Should().BeGreaterThan(260);

        try
        {
            sut.WriteGeneratedFile(filePath, "content");
            File.ReadAllText(sut.GetFileSystemPath(filePath)).Should().Be("content");

            sut.DeleteGeneratedFile(filePath);
            File.Exists(sut.GetFileSystemPath(filePath)).Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(rootFolder))
                Directory.Delete(rootFolder, true);
        }
    }
}
