using System;
using SeleniumResults.Models;
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
        public bool HasMidnightErrors { get; set; }
        public bool HasTooManyErrors { get; set; }
        public string UniqueId { get; set; }

        public bool IsProcessed { get; set; }
        public TestRunDao()
        {
        }

        public TestRunDao(TestRun data)
        {
            BuildNumber = data.TestRunMetaData.BuildNumber;
            Duration = data.TestRunMetaData.Duration;
            TestRunType = data.TestRunMetaData.TestRunType;
            FlytApplicationType = data.TestRunMetaData.FlytApplicationType;
            LastRun = data.TestRunMetaData.LastRun;
            OriginalFileName = data.TestRunMetaData.OriginalFileName;
            HasMidnightErrors = data.HasMidnightErrors;
            HasTooManyErrors = data.HasTooManyErrors;
            UniqueId = data.GetUniqueId();
        }
    }
}