using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;

namespace SeleniumResults.Models
{
    public class TestRun
    {
        public TestRun(string shortName, Application type, DateTime lastRun, List<SingleTestResult> results)
        {
            FileName = shortName;
            ApplicationType = type;
            LastRun = lastRun;
            Results = results;
            IsSuccessfull = results.All(x => !x.IsFailure);
            IsSelenium2 = results.First().IsSel2;
        }

        public bool IsSelenium2 { get; }
        public string FileName { get; }
        public Application ApplicationType { get; }
        public DateTime LastRun { get; }
        public List<SingleTestResult> Results { get; }
        public bool IsSuccessfull { get; }

        public string GetId()
        {
            return $"{ApplicationType}-{LastRun:yyyy-MM-dd HH:mm:ss}";
        }

        public override string ToString()
        {
            return $"(id={GetId()}, filename={FileName}, isSelenium2={IsSelenium2}, isSucessful={IsSuccessfull})";
        }

        protected bool Equals(TestRun other)
        {
            return ApplicationType == other.ApplicationType && LastRun == other.LastRun;
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
            return HashCode.Combine((int) ApplicationType, LastRun);
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