using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;

namespace Reqnroll.TestProjectGenerator.Driver
{
    public class BindingsDriver(TestProjectFolders _testProjectFolders)
    {
        public void CheckIsHookExecuted(string methodName, int expectedTimesExecuted)
        {
            int hookExecutionCount = GetHookExecutionCount(methodName);
            hookExecutionCount.Should().Be(expectedTimesExecuted, $"{methodName} executed that many times");
        }

        public int GetHookExecutionCount(string methodName)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            if (!File.Exists(_testProjectFolders.LogFilePath))
            {
                return 0;
            }

            string content = File.ReadAllText(_testProjectFolders.LogFilePath);
            content.Should().NotBeNull();

            var regex = new Regex($@"-> hook: {methodName}");

            return regex.Matches(content).Count;
        }

        private string GetLogFileLockPath() => _testProjectFolders.LogFilePath + ".lock";

        public void AcquireHookLock()
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            var pathToHookLogFile = GetLogFileLockPath();

            Directory.CreateDirectory(Path.GetDirectoryName(pathToHookLogFile)!);
            using (File.Open(pathToHookLogFile, FileMode.CreateNew))
            {
            }
        }

        public void ReleaseHookLock()
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();
            File.Delete(GetLogFileLockPath());
        }

        public async Task WaitForIsWaitingForHookLockAsync(string methodName)
        {
            for (var i = 0; i < 60 * 5; i++)
            {
                if (CheckIsWaitingForHookLock(methodName))
                {
                    return;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException($"No one is waiting for hook '{methodName}' lock for such a long time");
        }

        private bool CheckIsWaitingForHookLock(string methodName)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            if (!File.Exists(_testProjectFolders.LogFilePath))
            {
                return false;
            }

            var content = File.ReadAllText(_testProjectFolders.LogFilePath);
            content.Should().NotBeNull();

            var regex = new Regex($@"-> waiting for hook lock: {methodName}");

            return regex.Matches(content).Count == 1;
        }

        public IEnumerable<string> GetActualLogLines(string category)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            var lines = File.ReadAllLines(_testProjectFolders.LogFilePath);
            return lines.Where(l => l.StartsWith($"-> {category}:"));
        }

        public void CheckIsNotHookExecuted(string methodName, int timesExecuted)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            var content = File.ReadAllText(_testProjectFolders.LogFilePath);
            content.Should().NotBeNull();

            var regex = new Regex($@"-> hook: {methodName}");

            regex.Matches(content).Count.Should().NotBe(timesExecuted);
        }

        public IEnumerable<string> GetActualHookLines() => GetActualLogLines("hook");

        public void AssertHooksExecutedInOrder(params string[] methodNames)
        {
            var hookLines = GetActualHookLines();

            var methodNameLines = methodNames.Select(m => $"-> hook: {m}");
            hookLines.Should().ContainInOrder(methodNameLines);
        }

        public void AssertExecutedHooksEqual(params string[] methodNames)
        {
            var hookLines = GetActualHookLines();

            var methodNameLines = methodNames.Select(m => $"-> hook: {m}");
            hookLines.Should().Equal(methodNameLines);
        }

        private IEnumerable<string> GetActualStepLines() => GetActualLogLines("step");

        public void AssertStepsExecutedInOrder(params string[] methodNames)
        {
            var stepLines = GetActualStepLines();

            var methodNameLines = methodNames.Select(m => $"-> step: {m}");
            stepLines.Should().ContainInOrder(methodNameLines);
        }

        public void AssertExecutedStepsEqual(params string[] methodNames)
        {
            var stepLines = GetActualStepLines();

            var methodNameLines = methodNames.Select(m => $"-> step: {m}");
            stepLines.Should().Equal(methodNameLines);
        }
    }
}
