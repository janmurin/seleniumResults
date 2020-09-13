using System;
using SeleniumResults.Models.data;
using SeleniumResults.Models.enums;

namespace SeleniumResults.Repository.Models
{
    public class TestRunDao : DomainDao
    {
        public int BuildNumber { get; set; }
        public int Duration { get; set; }
        public TestRunType TestRunType { get; set; }
        public FlytApplication FlytApplicationType { get; set; }
        public DateTime LastRun { get; set; }
        public string OriginalFileName { get; set; }
        public string UniqueId { get; set; }

        public bool IsProcessed { get; set; }
        public TestRunDao()
        {
        }

        public TestRunDao(TestRun data, int version)
        {
            Version = version;
            BuildNumber = data.TestRunMetaData.BuildNumber;
            Duration = data.TestRunMetaData.Duration;
            TestRunType = data.TestRunMetaData.TestRunType;
            FlytApplicationType = data.TestRunMetaData.FlytApplicationType;
            LastRun = data.TestRunMetaData.LastRun;
            OriginalFileName = data.TestRunMetaData.OriginalFileName;
            UniqueId = data.GetUniqueId();
        }
    }
}