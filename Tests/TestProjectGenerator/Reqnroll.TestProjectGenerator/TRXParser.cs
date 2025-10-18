using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Reqnroll.TestProjectGenerator
{
    public class TRXParser
    {
        private readonly TestRunConfiguration _testRunConfiguration;
        private readonly XNamespace _xmlns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
        private readonly Regex _xunitPendingOrInconclusiveRegex = new Regex("(XUnitPendingStepException|XUnitInconclusiveException)");
        private readonly XName _unitTestResultElementName;
        private readonly XName _unitTestResultOutputElementName;
        private readonly XName _unitTestResultStdOutElementName;
        private readonly XName _unitTestResultInnerResultsElementName;
        private readonly XName _testRunElementName;
        private readonly XName _resultsElementName;

        public TRXParser(TestRunConfiguration testRunConfiguration)
        {
            _testRunConfiguration = testRunConfiguration;
            _unitTestResultElementName = _xmlns + "UnitTestResult";
            _unitTestResultOutputElementName = _xmlns + "Output";
            _unitTestResultStdOutElementName = _xmlns + "StdOut";
            _unitTestResultInnerResultsElementName = _xmlns + "InnerResults";
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
            var summaryElement = testRunElement.Element(_xmlns + "ResultSummary")
                                 ?? throw new InvalidOperationException("Invalid document; result summary element not found.");
            var summaryCountersElement = summaryElement?.Element(_xmlns + "Counters")
                                         ?? throw new InvalidOperationException("Invalid document; result summary counters element not found.");

            var totalAttribute = summaryCountersElement.Attribute("total");
            var executedAttribute = summaryCountersElement.Attribute("executed");
            var passedAttribute = summaryCountersElement.Attribute("passed");
            var failedAttribute = summaryCountersElement.Attribute("failed");
            var inconclusiveAttribute = summaryCountersElement.Attribute("inconclusive");

            int.TryParse(totalAttribute?.Value, out int total);
            int.TryParse(executedAttribute?.Value, out int executed);
            int.TryParse(passedAttribute?.Value, out int passed);
            int.TryParse(failedAttribute?.Value, out int failed);
            int.TryParse(inconclusiveAttribute?.Value, out int inconclusive);

            var runInfos = summaryElement.Element(_xmlns + "RunInfos")?.Elements(_xmlns + "RunInfo") ?? Enumerable.Empty<XElement>();
            var warnings = runInfos
                           .Where(ri => "Warning".Equals(ri.Attribute("outcome")?.Value, StringComparison.InvariantCultureIgnoreCase))
                           .Select(ri => ri.Element(_xmlns + "Text")?.Value ?? "[no warning text]")
                           .ToArray();

            var testResults = GetTestResults(testRunElement, _xmlns);
            var leafTestResults =
                testResults.Where(tr => tr.InnerResults.Count == 0)
                           .Concat(testResults.Where(tr => tr.InnerResults.Count > 0).SelectMany(tr => tr.InnerResults))
                           .ToArray();

            string trxOutput = leafTestResults.Select(r => r.StdOut).Aggregate(new StringBuilder(), (acc, c) => acc.AppendLine(c)).ToString();

            return new TestExecutionResult
            {
                ValidLicense = false,
                TestResults = testResults,
                LeafTestResults = leafTestResults,
                Output = output,
                ReportFiles = reportFiles.ToList(),
                TrxOutput = trxOutput,
                LogFileContent = logFileContent,
                Total = total,
                Executed = executed,
                Succeeded = passed,
                Failed = failed,
                Pending = inconclusive,
                Warnings = warnings
            };
        }

        private TestExecutionResult CalculateUnitTestProviderSpecificTestExecutionResult(TestExecutionResult testExecutionResult, TestRunConfiguration testRunConfiguration, XDocument trx)
        {
            switch (testRunConfiguration.UnitTestProvider)
            {
                case UnitTestProvider.xUnit: 
                case UnitTestProvider.xUnit3: 
                    return CalculateXUnitTestExecutionResult(testExecutionResult, trx);
                case UnitTestProvider.MSTest:
                case UnitTestProvider.MSTest4:
                    return CalculateMsTestTestExecutionResult(testExecutionResult);
                case UnitTestProvider.NUnit3: 
                case UnitTestProvider.NUnit4: 
                    return CalculateNUnitTestExecutionResult(testExecutionResult);
                case UnitTestProvider.TUnit:
                    return CalculateTUnitTestExecutionResult(testExecutionResult);
                default: throw new NotSupportedException($"The specified unit test provider is not supported: {testRunConfiguration.UnitTestProvider}");
            }
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

            testExecutionResult.Total = testExecutionResult.LeafTestResults.Length;
            testExecutionResult.Succeeded = testExecutionResult.LeafTestResults.Count(tr => tr.Outcome == "Passed");
            testExecutionResult.Failed = testExecutionResult.LeafTestResults.Count(tr => tr.Outcome == "Failed");
            testExecutionResult.Executed = testExecutionResult.Succeeded + testExecutionResult.Failed;
            testExecutionResult.Ignored = testExecutionResult.LeafTestResults.Count(r => r.Outcome == "NotExecuted" && !HasPendingError(r));
            testExecutionResult.Pending = testExecutionResult.LeafTestResults.Count(HasPendingError);
    
            return testExecutionResult;
        }

        private TestExecutionResult CalculateXUnitTestExecutionResult(TestExecutionResult testExecutionResult, XDocument trx)
        {
            testExecutionResult.Pending = GetXUnitPendingCount(trx.Element(_testRunElementName)?.Element(_resultsElementName));
            testExecutionResult.Failed -= testExecutionResult.Pending;
            testExecutionResult.Ignored = testExecutionResult.Total - testExecutionResult.Executed;

            return testExecutionResult;
        }

        private TestExecutionResult CalculateTUnitTestExecutionResult(TestExecutionResult testExecutionResult)
        {
            testExecutionResult.Ignored = GetTUnitIgnoredCount(testExecutionResult);
            testExecutionResult.Pending = testExecutionResult.Total - testExecutionResult.Executed - testExecutionResult.Ignored;

            return testExecutionResult;
        }

        private List<TestResult> GetTestResults(XElement testRunElement, XNamespace xmlns)
        {
            return GetTestResultsInternal(testRunElement.Element(xmlns + "Results"), xmlns);
        }

        private List<TestResult> GetTestResultsInternal(XElement testRunResultsElement, XNamespace xmlns)
        {
            var testResults = from unitTestResultElement in testRunResultsElement?.Elements(_unitTestResultElementName) ?? Enumerable.Empty<XElement>()
                let outputElement = unitTestResultElement.Element(_unitTestResultOutputElementName)
                let idAttribute = unitTestResultElement.Attribute("executionId")
                let testNameAttribute = unitTestResultElement.Attribute("testName")
                let outcomeAttribute = unitTestResultElement.Attribute("outcome")
                let stdOutElement = outputElement?.Element(_unitTestResultStdOutElementName)
                let errorInfoElement = outputElement?.Element(xmlns + "ErrorInfo")
                let errorMessage = errorInfoElement?.Element(xmlns + "Message")
                let innerResultsElement = unitTestResultElement.Element(_unitTestResultInnerResultsElementName)
                where idAttribute != null
                where outcomeAttribute != null
                let steps = ParseTestOutput(stdOutElement)
                select new TestResult
                {
                    Id = idAttribute.Value,
                    TestName = testNameAttribute.Value,
                    Outcome = outcomeAttribute.Value,
                    StdOut = stdOutElement?.Value,
                    Steps = steps.ToList(),
                    ErrorMessage = errorMessage?.Value,
                    InnerResults = GetTestResultsInternal(innerResultsElement, xmlns)
                };

            return testResults.ToList();
        }

        private static IEnumerable<TestStepResult> ParseTestOutput(XElement stdOutElement)
        {
            if (stdOutElement == null)
            {
                yield break;
            }

            var stdOutText = stdOutElement.Value;

            var logLines = stdOutText.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var primaryOutput = logLines.Where(line => line != "TestContext Messages:");

            TestStepResult step = null;

            foreach (var line in primaryOutput)
            {
                if (line.StartsWith("Given ") ||
                    line.StartsWith("When ") ||
                    line.StartsWith("Then ") ||
                    line.StartsWith("And "))
                {
                    if (step != null)
                    {
                        yield return step;
                    }

                    step = new TestStepResult { Step = line };
                }
                else
                {
                    step?.Output.Add(line);
                }
            }

            if (step != null)
            {
                yield return step;
            }
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

        private int GetTUnitIgnoredCount(TestExecutionResult testExecutionResult)
        {
            return testExecutionResult.TestResults.Count(
                r => r.Outcome == "NotExecuted"
                     && (r.ErrorMessage?.Contains("Ignored scenario") == true
                         || r.ErrorMessage?.Contains("Ignored feature") == true));
        }
    }
}
