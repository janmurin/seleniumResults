using System;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;
using Newtonsoft.Json;

namespace SeleniumResults.Repository.Models
{
    public class TestResultDao : DomainDao
    {
        public int TestRunId { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public TestResultType TestResultType { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsMidnightError { get; set; }
        public string SubtestsJson { get; set; }
        
        public TestRunDao TestRun { get; set; }

        public TestResultDao()
        {
        }

        public TestResultDao(SingleTestResult testResult, int testRunId)
        {
            TestRunId = testRunId;
            Name = testResult.Name;
            Time = testResult.Time;
            TestResultType = testResult.TestResultType;
            EndTime = testResult.EndTime;
            StartTime = testResult.StartTime;
            IsMidnightError = testResult.IsMidnightError;
            SubtestsJson = JsonConvert.SerializeObject(testResult.SubTests);
        }
    }
}