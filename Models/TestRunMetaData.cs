using System;
using SeleniumResults.Models.enums;

namespace SeleniumResults.Models
{
    public class TestRunMetaData
    {
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

        public override string ToString()
        {
            return $"(Duration={Duration}, OriginalFileName={OriginalFileName}, TestRunType={TestRunType}, FlytApplicationType={FlytApplicationType}), buildNumber={BuildNumber}, LastRun={LastRun}";
        }

        #region equals

        protected bool Equals(TestRunMetaData other)
        {
            return BuildNumber == other.BuildNumber && TestRunType == other.TestRunType && FlytApplicationType == other.FlytApplicationType &&
                   LastRun.Equals(other.LastRun);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestRunMetaData) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BuildNumber, (int) TestRunType, (int) FlytApplicationType, LastRun);
        }

        public static bool operator ==(TestRunMetaData left, TestRunMetaData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TestRunMetaData left, TestRunMetaData right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}