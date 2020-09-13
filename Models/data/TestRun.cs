using System.Collections.Generic;

namespace SeleniumResults.Models.data
{
    public class TestRun
    {
        public TestRunMetaData TestRunMetaData { get; }
        public List<TestResult> Results { get; }
        public TestRun(TestRunMetaData testRunMetaData, List<TestResult> results)
        {
            TestRunMetaData = testRunMetaData;
            Results = results;
        }
        public string GetUniqueId()
        {
            return $"{TestRunMetaData.TestRunType}-{TestRunMetaData.FlytApplicationType}-{TestRunMetaData.LastRun:yyyy-MM-dd HH:mm:ss}";
        }
    }
}