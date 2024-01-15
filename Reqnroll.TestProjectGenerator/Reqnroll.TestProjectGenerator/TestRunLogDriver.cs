using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Reqnroll.TestProjectGenerator
{
    public class TestRunLogDriver
    {
        private readonly VSTestExecutionDriver _vsTestExecutionDriver;
        private readonly TestProjectFolders _testProjectFolders;

        public TestRunLogDriver(VSTestExecutionDriver vsTestExecutionDriver, TestProjectFolders testProjectFolders)
        {
            _vsTestExecutionDriver = vsTestExecutionDriver;
            _testProjectFolders = testProjectFolders;
        }

        public void CheckLogMatchesRegexTimes(string regexString, int times, string logFilePathFromProjectFolder)
        {
            string fullLogFilePath = GetFullFilePathFromProjectFolder(logFilePathFromProjectFolder);
            string logContent = File.ReadAllText(fullLogFilePath);
            logContent.Should().NotBeNullOrEmpty($@"the trace log file ""{fullLogFilePath}"" should have been generated");

            var regex = new Regex(regexString, RegexOptions.Multiline);
            if (times > 0)
            {
                logContent.Should().MatchRegex(regexString);
            }

            if (times != int.MaxValue)
            {
                regex.Matches(logContent).Count.Should().Be(times);
            }
        }

        public void CheckLogContainsText(string text, string logFilePathFromProjectFolder)
        {
            string fullLogFilePath = GetFullFilePathFromProjectFolder(logFilePathFromProjectFolder);
            string logContent = File.ReadAllText(fullLogFilePath);
            logContent.Should().Contain(text);
        }

        public string GetFullFilePathFromProjectFolder(string relativeFilePathFromProjectFolder)
        {
            return Path.Combine(_testProjectFolders.ProjectFolder, relativeFilePathFromProjectFolder);
        }

        public void CheckLogContainsText(string text)
        {
            _vsTestExecutionDriver.LastTestExecutionResult.LogFileContent.Should().Contain(text);
        }
    }
}
