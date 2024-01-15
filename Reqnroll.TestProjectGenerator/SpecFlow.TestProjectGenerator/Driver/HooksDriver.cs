using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class HooksDriver
    {
        private readonly TestProjectFolders _testProjectFolders;

        public HooksDriver(TestProjectFolders testProjectFolders)
        {
            _testProjectFolders = testProjectFolders;
        }

        public void CheckIsHookExecuted(string methodName, int expectedTimesExecuted)
        {
            int hookExecutionCount = GetHookExecutionCount(methodName);
            hookExecutionCount.Should().Be(expectedTimesExecuted, $"{methodName} executed that many times");
        }

        public int GetHookExecutionCount(string methodName)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            string pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
            if (!File.Exists(pathToHookLogFile))
            {
                return 0;
            }

            string content = File.ReadAllText(pathToHookLogFile);
            content.Should().NotBeNull();

            var regex = new Regex($@"-> hook: {methodName}");

            return regex.Matches(content).Count;
        }

        public void AcquireHookLock()
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            var pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log.lock");

            Directory.CreateDirectory(Path.GetDirectoryName(pathToHookLogFile));
            using (File.Open(pathToHookLogFile, FileMode.CreateNew))
            {
            }
        }

        public void ReleaseHookLock()
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            var pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log.lock");

            File.Delete(pathToHookLogFile);
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

            var pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");

            if (!File.Exists(pathToHookLogFile))
            {
                return false;
            }

            var content = File.ReadAllText(pathToHookLogFile);
            content.Should().NotBeNull();

            var regex = new Regex($@"-> waiting for hook lock: {methodName}");

            return regex.Matches(content).Count == 1;
        }

        public void CheckIsNotHookExecuted(string methodName, int timesExecuted)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            var pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
            var content = File.ReadAllText(pathToHookLogFile);
            content.Should().NotBeNull();

            var regex = new Regex($@"-> hook: {methodName}");

            regex.Matches(content).Count.Should().NotBe(timesExecuted);
        }

        public void CheckIsHookExecutedInOrder(IEnumerable<string> methodNames)
        {
            _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

            var pathToHookLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
            var lines = File.ReadAllLines(pathToHookLogFile);
            var methodNameLines = methodNames.Select(m => $"-> hook: {m}");
            lines.Should().ContainInOrder(methodNameLines);
        }
    }
}
