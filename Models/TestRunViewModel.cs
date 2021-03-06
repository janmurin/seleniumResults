using System;
using System.Collections.Generic;
using System.Linq;
using SeleniumResults.Models.data;

namespace SeleniumResults.Models
{
    public class TestRunViewModel
    {
        public TestRunMetaData TestRunMetaData { get; }
        public List<TestResultViewModel> Results { get; }
        public bool HasMidnightErrors { get; }
        public bool HasSeleniumGridErrors { get; }
        public bool IsSeleniumGridErrorRun { get; }
        public bool HasTooManyErrors { get; }
        public bool IsPassed { get; }
        public bool IsSel1 { get; }
        public bool IsSel2 { get; }
        public int FailedCount { get; }
        public int SeleniumGridErrorCount { get; }
        public int TotalCount { get; }
        public string GetDurationMinutesString { get; }

        public TestRunViewModel(List<TestResultViewModel> results)
        {
            TestRunMetaData = results.First().TestResult.TestRunMetaData;
            Results = results;
            IsPassed = results.Any() && results.All(x => !x.IsFailed);
            HasMidnightErrors = results.Any(x => x.IsMidnightError);
            HasSeleniumGridErrors = results.Any(x => x.IsSeleniumGridError);
            HasTooManyErrors = !HasMidnightErrors && !HasSeleniumGridErrors && results.Count(x => x.IsFailed) > Constants.FAILURE_THRESHOLD;
            IsSel1 = results.Any() && results.First().IsSel1;
            IsSel2 = results.Any() && results.First().IsSel2;
            FailedCount = results.Any() ? results.Count(x => x.IsFailed) : 0;
            SeleniumGridErrorCount = results.Any() ? results.Count(x => x.IsSeleniumGridError) : 0;
            TotalCount = results.Any() ? results.Count(x => x.IsPassedOrFailed) : 0;
            GetDurationMinutesString = $"{results.Sum(x => x.GetDurationMinutes),0:0.00} min";
            IsSeleniumGridErrorRun = HasSeleniumGridErrors && SeleniumGridErrorCount >= 5 && SeleniumGridErrorCount > FailedCount / 2.0;
        }
    }
}