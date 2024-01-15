using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class TRXParser
    {
        private readonly TestRunConfiguration _testRunConfiguration;
        private readonly XNamespace _xmlns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
        private readonly Regex _xunitPendingOrInconclusiveRegex = new Regex("(XUnitPendingStepException|XUnitInconclusiveException)");
        private readonly XName _unitTestResultElementName;
        private readonly XName _unitTestResultOutputElementName;
        private readonly XName _unitTestResultStdOutElementName;
        private readonly XName _testRunElementName;
        private readonly XName _resultsElementName;

        public TRXParser(TestRunConfiguration testRunConfiguration)
        {
            _testRunConfiguration = testRunConfiguration;
            _unitTestResultElementName = _xmlns + "UnitTestResult";
            _unitTestResultOutputElementName = _xmlns + "Output";
            _unitTestResultStdOutElementName = _xmlns + "StdOut";
            _testRunElementName = _xmlns + "TestRun";
            _resultsElementName = _xmlns + "Results";
        }

        public TestExecutionResult ParseTRXFile(string trxFile, string output, IEnumerable<string> reportFiles, string logFileContent)
        {
            var testResultDocument = XDocument.Load(trxFile);

            return CalculateTestExecutionResultFromTrx(testResultDocument, _testRunConfiguration, output, reportFiles, logFileContent);
        }

        private TestExecutionResult CalculateTestExecutionResultFromTrx(XDocument trx, TestRunConfiguration testRunConfiguration, string output, IEnumerable<string> reportFiles, string logFileContent)
        {
            var testExecutionResult = GetCommonTestExecutionResult(trx, output, reportFiles, logFileContent);

            return CalculateUnitTestProviderSpecificTestExecutionResult(testExecutionResult, testRunConfiguration, trx);
        }

        private TestExecutionResult GetCommonTestExecutionResult(XDocument trx, string output, IEnumerable<string> reportFiles, string logFileContent)
        {
            var testRunElement = trx.Descendants(_testRunElementName).Single();
            var summaryElement = testRunElement.Element(_xmlns + "ResultSummary")?.Element(_xmlns + "Counters")
                                 ?? throw new InvalidOperationException("Invalid document; result summary counters element not found.");

            var totalAttribute = summaryElement.Attribute("total");
            var executedAttribute = summaryElement.Attribute("executed");
            var passedAttribute = summaryElement.Attribute("passed");
            var failedAttribute = summaryElement.Attribute("failed");
            var inconclusiveAttribute = summaryElement.Attribute("inconclusive");

            int.TryParse(totalAttribute?.Value, out int total);
            int.TryParse(executedAttribute?.Value, out int executed);
            int.TryParse(passedAttribute?.Value, out int passed);
            int.TryParse(failedAttribute?.Value, out int failed);
            int.TryParse(inconclusiveAttribute?.Value, out int inconclusive);

            var testResults = GetTestResults(testRunElement, _xmlns);
            string trxOutput = Enumerable.Select<TestResult, string>(testResults, r => r.StdOut).Aggregate(new StringBuilder(), (acc, c) => acc.AppendLine(c)).ToString();

            return new TestExecutionResult
            {
                ValidLicense = false,
                TestResults = testResults,
                Output = output,
                ReportFiles = reportFiles.ToList(),
                TrxOutput = trxOutput,
                LogFileContent = logFileContent,
                Total = total,
                Executed = executed,
                Succeeded = passed,
                Failed = failed,
                Pending = inconclusive,
            };
        }

        private TestExecutionResult CalculateUnitTestProviderSpecificTestExecutionResult(TestExecutionResult testExecutionResult, TestRunConfiguration testRunConfiguration, XDocument trx)
        {
            switch (testRunConfiguration.UnitTestProvider)
            {
                case UnitTestProvider.xUnit: return CalculateXUnitTestExecutionResult(testExecutionResult, trx);
                case UnitTestProvider.MSTest: return CalculateMsTestTestExecutionResult(testExecutionResult);
                case UnitTestProvider.NUnit3: return CalculateNUnitTestExecutionResult(testExecutionResult);
                case UnitTestProvider.SpecRun: return CalculateSpecRunTestExecutionResult(testExecutionResult);
                default: throw new NotSupportedException($"The specified unit test provider is not supported: {testRunConfiguration.UnitTestProvider}");
            }
        }

        private TestExecutionResult CalculateSpecRunTestExecutionResult(TestExecutionResult testExecutionResult)
        {
            bool FilterIgnored(TestResult testResult) => testResult.StdOut.Contains("-> Ignored");

            bool FilterPending(TestResult testResult) => testResult.StdOut.Contains("TechTalk.SpecRun.PendingTestException")
                                                         || testResult.StdOut.Contains("No matching step definition found for the step.");

            var testResultsWithOutput = testExecutionResult.TestResults.Where(tr => !(tr?.StdOut is null)).ToArray();

            testExecutionResult.Ignored = testResultsWithOutput.Where(FilterIgnored).Count();
            testExecutionResult.Pending = testResultsWithOutput.Where(FilterPending).Count();

            return testExecutionResult;
        }

        private TestExecutionResult CalculateNUnitTestExecutionResult(TestExecutionResult testExecutionResult)
        {
            testExecutionResult.Ignored = GetNUnitIgnoredCount(testExecutionResult);
            testExecutionResult.Pending = testExecutionResult.Total - testExecutionResult.Executed - testExecutionResult.Ignored;

            return testExecutionResult;
        }

        private TestExecutionResult CalculateMsTestTestExecutionResult(TestExecutionResult testExecutionResult)
        {
            bool HasPendingError(TestResult r) => r.ErrorMessage != null && r.ErrorMessage.Contains("Assert.Inconclusive failed. One or more step definitions are not implemented yet.");

            testExecutionResult.Ignored = testExecutionResult.TestResults.Count(r => r.Outcome == "NotExecuted" && !HasPendingError(r));
            testExecutionResult.Pending = testExecutionResult.TestResults.Count(r => HasPendingError(r));
    
            return testExecutionResult;
        }

        private TestExecutionResult CalculateXUnitTestExecutionResult(TestExecutionResult testExecutionResult, XDocument trx)
        {
            testExecutionResult.Pending = GetXUnitPendingCount(trx.Element(_testRunElementName)?.Element(_resultsElementName));
            testExecutionResult.Failed -= testExecutionResult.Pending;
            testExecutionResult.Ignored = testExecutionResult.Total - testExecutionResult.Executed;

            return testExecutionResult;
        }

        private List<TestResult> GetTestResults(XElement testRunElement, XNamespace xmlns)
        {
            var testResults = from unitTestResultElement in testRunElement.Element(xmlns + "Results")?.Elements(_unitTestResultElementName) ?? Enumerable.Empty<XElement>()
                let outputElement = unitTestResultElement.Element(_unitTestResultOutputElementName)
                let idAttribute = unitTestResultElement.Attribute("executionId")
                let outcomeAttribute = unitTestResultElement.Attribute("outcome")
                let stdOutElement = outputElement?.Element(_unitTestResultStdOutElementName)
                let errorInfoElement = outputElement?.Element(xmlns + "ErrorInfo")
                let errorMessage = errorInfoElement?.Element(xmlns + "Message")
                where idAttribute != null
                where outcomeAttribute != null
                select new TestResult
                {
                    Id = idAttribute.Value,
                    Outcome = outcomeAttribute.Value,
                    StdOut = stdOutElement?.Value,
                    ErrorMessage = errorMessage?.Value
                };

            return testResults.ToList();
        }

        private int GetNUnitIgnoredCount(TestExecutionResult testExecutionResult)
        {
            return testExecutionResult.TestResults.Count(
                r => r.Outcome == "NotExecuted"
                     && (r.ErrorMessage?.Contains("Ignored scenario") == true
                         || r.ErrorMessage?.Contains("Ignored feature") == true));
        }

        public int GetXUnitPendingCount(XElement resultsElement)
        {
            var pendingOrInconclusiveTests =
                from unitTestResult in resultsElement?.Elements(_unitTestResultElementName) ?? new XElement[0]
                let unitTestOutputElement = unitTestResult?.Element(_unitTestResultOutputElementName)
                let unitTestOutput = unitTestOutputElement?.Value ?? ""
                where _xunitPendingOrInconclusiveRegex.IsMatch(unitTestOutput)
                select _xunitPendingOrInconclusiveRegex;

            return pendingOrInconclusiveTests.Count();
        }
    }
}
