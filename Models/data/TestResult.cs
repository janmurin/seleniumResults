using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository.Models;

namespace SeleniumResults.Models.data
{
    public class TestResult
    {
        public TestRunMetaData TestRunMetaData { get; }
        public string Name { get; }
        public string Time { get; }
        public TestResultType TestResultType { get; }
        public DateTime EndTime { get; }
        public DateTime StartTime { get; }
        public List<SubTest> SubTests { get; }

        public TestResult(TestRunMetaData metaData, string name, TestResultType type, DateTime start, DateTime end, List<SubTest> subTests)
        {
            TestRunMetaData = metaData;
            Name = name;
            TestResultType = type;
            Time = end.ToString("yyyy-MM-dd HH:mm:ss");
            EndTime = end;
            StartTime = start;
            SubTests = subTests;
        }

        public TestResult(TestResultDao testResultDao, TestRunDao testRunDao)
        {
            TestRunMetaData = new TestRunMetaData(testRunDao);
            Name = testResultDao.Name;
            Time = testResultDao.Time;
            TestResultType = testResultDao.TestResultType;
            EndTime = testResultDao.EndTime;
            StartTime = testResultDao.StartTime;
            SubTests = JsonConvert.DeserializeObject<List<SubTest>>(testResultDao.SubtestsJson);
        }
    }
}