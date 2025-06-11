using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Reqnroll.Formatters.Tests
{
    internal class CucumberMessagesHTMLGenerationValidator
    {
        private string[] _generatedHtml;
        private string[] _expectedJson;

        public CucumberMessagesHTMLGenerationValidator(string[] generatedHtml, string[] expectedJsonContent)
        {
            _generatedHtml = generatedHtml;
            _expectedJson = expectedJsonContent;
        }

        public void GeneratedHtmlContentMatchesJsonMessages() {

            var actual = ExtractMessagesFromHtml(_generatedHtml)!.Replace(@"\/", "/");
            var expected = string.Join(',', _expectedJson);
            actual.Should().Be(expected);
        }

        public void GeneratedHtmlProperlyReflectsExpectedMessages()
        {
            using (new AssertionScope("Html Validates"))
            {
                GeneratedHtmlContentMatchesJsonMessages();
            }
        }

        private string? ExtractMessagesFromHtml(string[] html)
        {
            var line = html.Where(s => s.StartsWith("window.CUCUMBER_MESSAGES"))
                       .Select(s => s)
                       .FirstOrDefault();
            var start = line!.IndexOf('[');
            return line.Substring(start + 1, line.Length - start - 3);

        }
    }
}
