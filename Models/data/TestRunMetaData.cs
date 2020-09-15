using System;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository.Models;

namespace SeleniumResults.Models.data
{
    public class TestRunMetaData
    {
        public int Id { get; }
        public int BuildNumber { get; }
        public int Duration { get; }
        public TestRunType TestRunType { get; }
        public FlytApplication FlytApplicationType { get; }
        public DateTime LastRun { get; }
        public string OriginalFileName { get; }

        public TestRunMetaData(string fileName, FlytApplication applicationType, DateTime lastRun, TestRunType testRunType, int buildNumber, int duration)
        {
            OriginalFileName = fileName;
            FlytApplicationType = applicationType;
            LastRun = lastRun;
            TestRunType = testRunType;
            BuildNumber = buildNumber;
            Duration = duration/60;
        }

        public TestRunMetaData(TestRunDao testRunDao)
        {
            Id = testRunDao.Id;
            BuildNumber = testRunDao.BuildNumber;
            Duration = testRunDao.Duration;
            TestRunType = testRunDao.TestRunType;
            FlytApplicationType = testRunDao.FlytApplicationType;
            LastRun = testRunDao.LastRun;
            OriginalFileName = testRunDao.OriginalFileName;
        }
    }
}