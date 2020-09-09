using Newtonsoft.Json;
using SeleniumResults.Models.enums;

namespace SeleniumResults.Models
{
    public class SubTest
    {
        public string Name { get; }
        public TestResultType TestResultType { get; }
        public string Message { get; }
        public int SubTestNumber { get; }

        public SubTest(string name, TestResultType testResultType, string message, int subTestNumber)
        {
            Name = name;
            TestResultType = testResultType;
            Message = message;
            SubTestNumber = subTestNumber;
        }

        [JsonIgnore]
        public bool IsPassed => TestResultType == TestResultType.Passed;
        [JsonIgnore]
        public bool IsFailed => TestResultType == TestResultType.Failed;
        [JsonIgnore]
        public bool IsSkipped => TestResultType == TestResultType.Skipped;
    }
}