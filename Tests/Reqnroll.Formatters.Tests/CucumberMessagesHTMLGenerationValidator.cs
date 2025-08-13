using FluentAssertions;
using FluentAssertions.Execution;

namespace Reqnroll.Formatters.Tests;

internal class CucumberMessagesHtmlGenerationValidator
{
    private readonly string[] _generatedHtml;
    private readonly string[] _expectedJson;

    public CucumberMessagesHtmlGenerationValidator(string[] generatedHtml, string[] expectedJsonContent)
    {
        _generatedHtml = generatedHtml;
        _expectedJson = expectedJsonContent;
    }

    public void GeneratedHtmlContentMatchesJsonMessages() {

        var actual = ExtractMessagesFromHtml(_generatedHtml);
        var expected = string.Join(',', _expectedJson).Replace(@"<", "\\x3C");
        actual.Should().Be(expected);
    }

    public void GeneratedHtmlProperlyReflectsExpectedMessages()
    {
        using (new AssertionScope("Html Validates"))
        {
            GeneratedHtmlContentMatchesJsonMessages();
        }
    }

    private string ExtractMessagesFromHtml(string[] html)
    {
        var line = html.First(s => s.StartsWith("window.CUCUMBER_MESSAGES"));
        var start = line.IndexOf('[');
        return line.Substring(start + 1, line.Length - start - 3);
    }
}