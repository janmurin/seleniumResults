using System;
using SeleniumResults.Models.enums;

namespace SeleniumResults.Models
{
    public class SingleTestResult
    {
        public TestRunMetaData TestRunData { get; }
        public string Name { get; }
        public string Time { get; }
        public TestResultType TestResultType { get; }

        public SingleTestResult(TestRunMetaData testRunMetaData, string name, TestResultType testResultType, string time)
        {
            TestRunData = testRunMetaData;
            Name = name;
            TestResultType = testResultType;
            Time = time;
        }

        public bool IsPassed => TestResultType == TestResultType.Passed;
        public bool IsFailed => TestResultType == TestResultType.Failed;
        public bool IsPassedOrSkipped => TestResultType == TestResultType.Passed || TestResultType == TestResultType.Skipped;
        public bool IsPassedOrFailed => TestResultType == TestResultType.Failed || TestResultType == TestResultType.Passed;

        public override string ToString()
        {
            return $"time-{Time}, file-{TestRunData.OriginalFileName}, result-{TestResultType}, app-{TestRunData.FlytApplicationType}, buildNumber-{TestRunData.BuildNumber}";
        }

        #region equals

        protected bool Equals(SingleTestResult other)
        {
            return Equals(TestRunData, other.TestRunData) && Name == other.Name && Time == other.Time;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SingleTestResult) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TestRunData, Name, Time);
        }

        public static bool operator ==(SingleTestResult left, SingleTestResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SingleTestResult left, SingleTestResult right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}