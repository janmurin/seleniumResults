namespace SeleniumResults.Models
{
    public class SingleTestResult
    {
        public string Name { get; set; }
        public string BuildNumber { get; set; }
        public string Time { get; set; }
        public string OriginalFile { get; set; }
        public TestResultType TestResultType { get; set; }
        public TestRunType TestRunType { get; set; }
        public bool IsFailure => TestResultType == TestResultType.Failed;
        public bool IsPassedOrFailed => TestResultType == TestResultType.Failed || TestResultType == TestResultType.Passed;
        
        protected bool Equals(SingleTestResult other)
        {
            return Time == other.Time;
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
            return (Time != null ? Time.GetHashCode() : 0);
        }

        public static bool operator ==(SingleTestResult left, SingleTestResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SingleTestResult left, SingleTestResult right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"time-{Time}, file-{OriginalFile}, result-{TestResultType}, buildNumber-{BuildNumber}";
        }
    }
    
    
}