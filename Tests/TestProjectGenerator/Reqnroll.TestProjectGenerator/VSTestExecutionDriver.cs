using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator
{
    public class VSTestExecutionDriver
    {
        private readonly TestProjectFolders _testProjectFolders;
        private readonly IOutputWriter _outputWriter;
        private readonly TestRunConfiguration _testRunConfiguration;
        private readonly TRXParser _trxParser;
        private readonly TestSuiteEnvironmentVariableGenerator _testSuiteEnvironmentVariableGenerator;
        private readonly UriCleaner _uriCleaner;

        private const string BeginOfTrxFileLine = "Results File: ";
        private const string BeginOfLogFileLine = "Log file: ";
        private const string BeginOfReportFileLine = @"Report file: ";
        private const string DotnetTestPath = "dotnet";
        private static readonly Regex sWhitespace = new Regex(@"\s+");

        public VSTestExecutionDriver(
            TestProjectFolders testProjectFolders,
            IOutputWriter outputWriter,
            TestRunConfiguration testRunConfiguration,
            TRXParser trxParser,
            TestSuiteEnvironmentVariableGenerator testSuiteEnvironmentVariableGenerator)
        {
            _testProjectFolders = testProjectFolders;
            _outputWriter = outputWriter;
            _testRunConfiguration = testRunConfiguration;
            _uriCleaner = new UriCleaner();
            _trxParser = trxParser;
            _testSuiteEnvironmentVariableGenerator = testSuiteEnvironmentVariableGenerator;
        }

        /// <summary>
        /// Gets the test execution result of the test project.
        /// </summary>
        public TestExecutionResult LastTestExecutionResult { get; private set; }
        public IReadOnlyDictionary<string, TestExecutionResult> TestExecutionResultFiles { get; private set; }

        public string RunSettingsFile { get; set; }
        public string Filter { get; set; }

        public void CheckIsBindingMethodExecuted(string methodName, int timesExecuted)
        {
            string logFileContent = File.ReadAllText(_testProjectFolders.LogFilePath, Encoding.UTF8);

            var regex = new Regex($"-> step: {methodName}");

            regex.Match(logFileContent).Success.Should().BeTrue($"method {methodName} was not executed.");
            regex.Matches(logFileContent).Count.Should().Be(timesExecuted);
        }

        public void CheckOutputContainsText(string text)
        {
            LastTestExecutionResult.Output.Should().NotBeNull()
                                   .And.Subject.Should().Contain(text);
        }

        public void CheckAnyOutputContainsText(string text)
        {
            var textWithoutWhitespace = WithoutWhitespace(text);
            bool trxContainsEntry = WithoutWhitespace(LastTestExecutionResult.TrxOutput).Contains(textWithoutWhitespace);
            bool outputContainsEntry = WithoutWhitespace(LastTestExecutionResult.Output).Contains(textWithoutWhitespace);
            bool containsAtAll = trxContainsEntry || outputContainsEntry;
            containsAtAll.Should().BeTrue($"either Trx output or program output should contain '{text}'. Trx Output is: {LastTestExecutionResult.TrxOutput}");
        }

        public static string WithoutWhitespace(string input)
        {
            return sWhitespace.Replace(input, string.Empty);
        }

        public TestExecutionResult ExecuteTests()
        {
            var task = ExecuteTestsInternalAsync(async (processHelper, parameters) =>
                                                     processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, DotnetTestPath, parameters.argumentsFormat, parameters.environmentVariables));

            return task.Result;
        }

        public async Task<TestExecutionResult> ExecuteTestsAsync()
        {
            return await ExecuteTestsInternalAsync(async (processHelper, parameters) =>
                                                       await processHelper.RunProcessAsync(_outputWriter, _testProjectFolders.PathToSolutionDirectory, DotnetTestPath, parameters.argumentsFormat, parameters.environmentVariables));
        }

        private async Task<TestExecutionResult> ExecuteTestsInternalAsync(Func<ProcessHelper, (string argumentsFormat, IReadOnlyDictionary<string, string> environmentVariables), Task<ProcessResult>> runProcessAction)
        {
            var envVariables = _testSuiteEnvironmentVariableGenerator.GenerateEnvironmentVariables();

            var processHelper = new ProcessHelper();
            string arguments = $"test {GenerateDotnetTestsArguments()}";
            ProcessResult processResult;
            try
            {
                processResult = processHelper.RunProcess(_outputWriter, _testProjectFolders.PathToSolutionDirectory, DotnetTestPath, arguments, envVariables);
            }
            catch (Exception)
            {
                Console.WriteLine($"running {DotnetTestPath} failed - {_testProjectFolders.CompiledAssemblyPath} {DotnetTestPath} {arguments}");
                throw;
            }

            string output = processResult.CombinedOutput;

            var lines = output.SplitByString(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            string[] trxFilePaths;
            if (_testRunConfiguration.UnitTestProvider == UnitTestProvider.TUnit)
                trxFilePaths = FindFilePath(lines, ".trx", "- ").ToArray();
            else
                trxFilePaths = FindFilePath(lines, ".trx", BeginOfTrxFileLine).ToArray();
            var logFiles = FindFilePath(lines, ".log", BeginOfLogFileLine).ToArray();

            string logFileContent =
                logFiles.Length == 1
                ? File.ReadAllText(_uriCleaner.ConvertSlashes(_uriCleaner.StripSchema(Uri.UnescapeDataString(logFiles.Single()))))
                : string.Empty;

            var reportFiles = GetReportFiles(output);

            trxFilePaths.Should().HaveCountGreaterOrEqualTo(1, $"at least one TRX file should be generated by the test run; these have been generated:{Environment.NewLine}{string.Join(Environment.NewLine, trxFilePaths)}");

            var trxFiles = from trxFilePath in trxFilePaths
                              let trx = _trxParser.ParseTRXFile(trxFilePath, output, reportFiles, logFileContent)
                              select (trxFilePath, trx);

            TestExecutionResultFiles = trxFiles.ToDictionary(trxFile => trxFile.trxFilePath, trxFile => trxFile.trx);

            LastTestExecutionResult = TestExecutionResultFiles.First(f => f.Key.Contains(_testProjectFolders.ProjectFolder)).Value;

            return LastTestExecutionResult;
        }

        private IEnumerable<string> GetReportFiles(string output)
        {
            return output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                         .Where(i => i.StartsWith(BeginOfReportFileLine))
                         .Select(i => i.Substring(BeginOfReportFileLine.Length))
                         .Select(i => new Uri(i).AbsolutePath);
        }

        private IEnumerable<string> FindFilePath(string[] lines, string ending, string starting)
        {
            return from l in lines
                   let trimmed = l.Trim()
                   let start = trimmed.IndexOf(starting)
                   where trimmed.Contains(starting)
                   where trimmed.EndsWith(ending)
                   select trimmed.Substring(start + starting.Length);
        }

        public string GenerateDotnetTestsArguments()
        {
            var argumentsBuilder = new StringBuilder("--no-build ");

            argumentsBuilder.Append($" {GenerateVerbosityParameter("n")}");

            string additionalPackagesFoldersParameters = GenerateAdditionalPackagesFoldersParameters();
            if (additionalPackagesFoldersParameters is string additionalPackagesFoldersParametersString)
            {
                argumentsBuilder.Append($" {additionalPackagesFoldersParametersString}");
            }

            if (Filter.IsNotNullOrEmpty())
            {
                argumentsBuilder.Append($" --filter \"{Filter}\"");
            }

            if (RunSettingsFile.IsNotNullOrWhiteSpace())
            {
                var pathToRunSettingsFile = Path.Combine(_testProjectFolders.ProjectFolder, RunSettingsFile);
                argumentsBuilder.Append($" --settings \"{pathToRunSettingsFile}\"");
            }

            //string pathToReqnrollProject = Path.Combine(_testProjectFolders.ProjectFolder, $"{Path.GetFileName(_testProjectFolders.ProjectFolder)}.csproj");
            //argumentsBuilder.Append($@" ""{pathToReqnrollProject}""");
            argumentsBuilder.Append($@" ""{_testProjectFolders.PathToSolutionFile}""");

            argumentsBuilder.Append($" {GenerateTrxLoggerParameter()}");

            return argumentsBuilder.ToString();
        }

        public string GenerateTrxLoggerParameter()
        {
            if (_testRunConfiguration.UnitTestProvider == UnitTestProvider.TUnit)
                return "-- -report-trx";
            return "--logger trx";
        }

        public string GenerateVerbosityParameter(string verbosity)
        {
            return $"-v {verbosity}";
        }

        public string GenerateAdditionalPackagesFoldersParameters()
        {
            return null;
        }
    }
}
