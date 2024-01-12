using System;
using System.IO;
using FluentAssertions;
using Reqnroll.Tools.MsBuild.Generation;
using Xunit;

namespace Reqnroll.GeneratorTests
{
    public class MsBuildTaskLogOutputTests
    {

        [Theory]
        [InlineData("Some logging message", "[Reqnroll] Some logging message")]
        [InlineData("{0} should be handled correctly", "[Reqnroll] {0} should be handled correctly")]
        public void LogWithNameTag_Message_ShouldLogNameTagAndMessage(string message, string expected)
        {
            // ARRANGE
            using (var sw = new StringWriter())
            {
                Action<string, object[]> GetMockMethod(StringWriter writer) => (s, _) => writer.Write(s);

                // ACT
                LogExtensions.LogWithNameTag(null, GetMockMethod(sw), message);

                // ASSERT
                sw.ToString().Should().Be(expected);
            }
        }

        [Theory]
        [InlineData("{0} was thrown", "[Reqnroll] ArgumentException was thrown", nameof(ArgumentException))]
        [InlineData("{0} because {1}", "[Reqnroll] 1 because 2", 1, 2 )]
        public void LogWithNameTag_Message_Parameters_ShouldLogNameTagAndFullMessage(
            string message,
            string expected,
            params object[] messageArgs)
        {
            // ARRANGE
            using (var sw = new StringWriter())
            {
                Action<string, object[]> GetMockMethod(StringWriter writer) => writer.Write;

                // ACT
                LogExtensions.LogWithNameTag(null, GetMockMethod(sw), message, messageArgs);

                // ASSERT
                sw.ToString().Should().Be(expected);
            }
        }
    }
}
