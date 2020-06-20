using System;

namespace SeleniumResults.Models
{
    public class TestRunMetaData
    {
        public string BuildNumber { get; }
        public TestRunType TestRunType { get; }
        public FlytApplication FlytApplicationType { get; }
        public DateTime LastRun { get; }
        public string OriginalFileName { get; }

        public TestRunMetaData(string fileName, FlytApplication applicationType, DateTime lastRun, TestRunType testRunType, string buildNumber)
        {
            OriginalFileName = fileName;
            FlytApplicationType = applicationType;
            LastRun = lastRun;
            TestRunType = testRunType;
            BuildNumber = buildNumber;
        }
    }
}