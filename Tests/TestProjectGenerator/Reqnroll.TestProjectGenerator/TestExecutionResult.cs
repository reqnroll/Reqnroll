using System;
using System.Collections.Generic;

namespace Reqnroll.TestProjectGenerator
{
    public class TestExecutionResult
    {
        public int Total { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public int Pending { get; set; }
        public int Ignored { get; set; }
        public string Output { get; set; }
        public string TrxOutput { get; set; }
        public int Executed { get; set; }

        public List<TestResult> TestResults { get; set; }
        public TestResult[] LeafTestResults { get; set; }
        public List<string> ReportFiles { get; set; }
        public int Warning { get; set; }
        public string LogFileContent { get; set; }
        public bool ValidLicense { get; set; }
        public int ExitCode { get; set; }


        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string[] Warnings { get; set; }
    }

    public class TestResult
    {
        public string Id { get; set; }
        public string Outcome { get; set; }
        public string StdOut { get; set; }
        public string TestName { get; set; }
        public string Feature { get; set; }
        public int ScheduleOrder { get; set; }
        public List<TestStepResult> Steps { get; set; }
        public int ExecutionCount { get; set; }
        public string ErrorMessage { get; set; }
        public List<TestResult> InnerResults { get; set; }
    }

    public class TestStepResult
    {
        public string Step { get; set; }
        public string Error { get; set; }
        public string Result { get; set; }
        public List<string> Output { get; } = [];
    }
}