using SeleniumResults.Models.enums;

namespace SeleniumResults.Models
{
    public class SubTest
    {
        public string Name { get; }
        public TestResultType TestResultType { get; }
        public string Message { get; }
        
        public SubTest(string name, TestResultType testResultType, string message)
        {
            Name = name;
            TestResultType = testResultType;
            Message = message;
        }

        public bool IsPassed => TestResultType == TestResultType.Passed;
        public bool IsFailed => TestResultType == TestResultType.Failed;
        public bool IsSkipped => TestResultType == TestResultType.Skipped;
    }
}