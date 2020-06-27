using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;

namespace SeleniumResults.webreporting.ViewModels
{
    public class SeleniumTestViewModel
    {
        public SingleTestStats TestStats { get; }
        public string FailureProgressJson { get; }
        public string BuildNumbersArrayJson { get; }
        
        public SeleniumTestViewModel(SingleTestStats stats)
        {
            TestStats = stats;
            // var buildNumbers = TestStats.Results
            //     .Where(x => x.TestRunMetaData.LastRun >= Constants.TEST_RESULTS_START_DATETIME)
            //     .Select(x => x.TestRunMetaData.BuildNumber)
            //     .OrderBy(x => x)
            //     .ToList();
            // BuildNumbersArrayJson = JsonSerializer.Serialize(buildNumbers);
            //
            // var failureProgress = TestStats.LastXBuildsDict.GroupBy().Select(x => x.Value.FailureRate).Reverse().ToList();
            // FailureProgressJson = JsonSerializer.Serialize(failureProgress);
        }
    }
}