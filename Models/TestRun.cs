using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;

namespace SeleniumResults.Models
{
    public class TestRun
    {
        public TestRunMetaData TestRunMetaData { get; }
        public List<SingleTestResult> Results { get; }
        public bool IsPassed { get; }
        public bool HasTooManyFailures { get; }

        public TestRun(TestRunMetaData testRunMetaData, List<SingleTestResult> results)
        {
            TestRunMetaData = testRunMetaData;
            Results = results;
            IsPassed = results.Any() && results.All(x => x.IsPassedOrSkipped);
            HasTooManyFailures = results.Count(x => x.IsFailed) > Constants.FAILURE_THRESHOLD;
        }

        public string GetId()
        {
            return $"{TestRunMetaData.FlytApplicationType}-{TestRunMetaData.LastRun:yyyy-MM-dd HH:mm:ss}";
        }
        
        public override string ToString()
        {
            return $"(id={GetId()}, IsPassed={IsPassed}), TestRunMetaData={TestRunMetaData}";
        }

        #region equals

        protected bool Equals(TestRun other)
        {
            return TestRunMetaData.FlytApplicationType == other.TestRunMetaData.FlytApplicationType
                   && TestRunMetaData.LastRun == other.TestRunMetaData.LastRun;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestRun) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) TestRunMetaData.FlytApplicationType, TestRunMetaData.LastRun);
        }

        public static bool operator ==(TestRun left, TestRun right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TestRun left, TestRun right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}