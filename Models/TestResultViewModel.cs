using System;
using System.Linq;
using SeleniumResults.Models.data;
using SeleniumResults.Models.enums;

namespace SeleniumResults.Models
{
    public class TestResultViewModel
    {
        public TestResult TestResult { get; }
        public bool IsMidnightError { get; }
        public bool IsSeleniumGridError { get; }
        public bool IsPassed => TestResult.TestResultType == TestResultType.Passed;
        public bool IsFailed => TestResult.TestResultType == TestResultType.Failed;
        public bool IsSkipped => TestResult.TestResultType == TestResultType.Skipped;
        public bool IsPassedOrSkipped => TestResult.TestResultType == TestResultType.Passed || TestResult.TestResultType == TestResultType.Skipped;
        public bool IsPassedOrFailed => TestResult.TestResultType == TestResultType.Failed || TestResult.TestResultType == TestResultType.Passed;
        public bool IsSel1 => TestResult.TestRunMetaData.TestRunType == TestRunType.Selenium;
        public bool IsSel2 => TestResult.TestRunMetaData.TestRunType == TestRunType.Selenium2;
        public double GetDurationMinutes => (TestResult.EndTime - TestResult.StartTime).TotalMinutes;
        public string GetDurationMinutesString => $"{GetDurationMinutes,0:0.00} min";
        public int GetDurationSeconds => (int) (TestResult.EndTime - TestResult.StartTime).TotalSeconds;

        public TestResultViewModel(TestResult testResult)
        {
            TestResult = testResult;
            if (TestResult.TestRunMetaData.TestRunType == TestRunType.Selenium2)
            {
                IsMidnightError = TestResult.StartTime.Hour >= 22 &&
                                  TestResult.SubTests
                                      .Any(x => x.Message.ToLower() == "OneTimeSetUp: System.NullReferenceException : Object Reference Not Set To An Instance Of An Object.".ToLower());
                IsSeleniumGridError = TestResult.SubTests
                    .Any(x => x.Message.ToLower().Contains("http://flytapp901.flyttest.visma.com:4444".ToLower()));
            }
            else if (TestResult.TestRunMetaData.TestRunType == TestRunType.API)
            {
                IsMidnightError = TestResult.StartTime.Hour >= 22 &&
                                  TestResult.SubTests
                                      .Any(x => x.Message
                                                    .ToLower()
                                                    .StartsWith(
                                                        "System.NullReferenceException : Object Reference Not Set To An Instance Of An Object. ->    At Flow.Tests.API.Hooks.Hooks.CreateNewCustomer()"
                                                            .ToLower())
                                                || x.Message.ToLower()
                                                    .StartsWith(
                                                        "System.Exception : Unable To Create Administrator. Start Date Can Not Be Before Today ->    At Flow.Tests.API.Hooks.Hooks.CreateNewCustomer()"
                                                            .ToLower()));
            }
        }
    }
}