using System;
using SeleniumResults.Models.enums;
using Newtonsoft.Json;
using SeleniumResults.Models.data;

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
        public string SubtestsJson { get; set; }
        public TestRunType TestRunType { get; set; }
        public FlytApplication FlytApplicationType { get; set; }

        public TestResultDao()
        {
        }

        public TestResultDao(TestResult testResult, int testRunId, int version)
        {
            Version = version;
            TestRunId = testRunId;
            Name = testResult.Name;
            Time = testResult.Time;
            TestResultType = testResult.TestResultType;
            EndTime = testResult.EndTime;
            StartTime = testResult.StartTime;
            SubtestsJson = JsonConvert.SerializeObject(testResult.SubTests);
            TestRunType = testResult.TestRunMetaData.TestRunType;
            FlytApplicationType = testResult.TestRunMetaData.FlytApplicationType;
        }
    }
}