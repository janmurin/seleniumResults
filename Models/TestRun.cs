using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;

namespace SeleniumResults.Models
{
    public class TestRun
    {
        public TestRun(string fileName, FlytApplication type, DateTime lastRun, List<SingleTestResult> results, TestRunType testRunType, string buildNumber)
        {
            FileName = fileName;
            FlytApplicationType = type;
            LastRun = lastRun;
            Results = results;
            IsSuccessfull = results.All(x => !x.IsFailure);
            TestRunType = testRunType;
            BuildNumber = buildNumber;
        }

        public string BuildNumber { get; set; }
        public TestRunType TestRunType { get; }
        public string FileName { get; }
        public FlytApplication FlytApplicationType { get; }
        public DateTime LastRun { get; }
        public List<SingleTestResult> Results { get; set; }
        public bool IsSuccessfull { get; }

        public string GetId()
        {
            return $"{FlytApplicationType}-{LastRun:yyyy-MM-dd HH:mm:ss}";
        }

        public override string ToString()
        {
            return $"(id={GetId()}, filename={FileName}, TestRunType={TestRunType}, isSucessful={IsSuccessfull})";
        }

        protected bool Equals(TestRun other)
        {
            return FlytApplicationType == other.FlytApplicationType && LastRun == other.LastRun;
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
            return HashCode.Combine((int) FlytApplicationType, LastRun);
        }

        public static bool operator ==(TestRun left, TestRun right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TestRun left, TestRun right)
        {
            return !Equals(left, right);
        }
    }
}