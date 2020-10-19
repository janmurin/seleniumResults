using System.Collections.Generic;
using System.Linq;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;

namespace SeleniumResults.webreporting.ViewModels
{
    public sealed class SeleniumRunsViewModel : LastXBuildStatsByApp
    {
        public List<TestRunViewModel> TestRuns { get; }
        public override HashSet<TestResultViewModel> Results { get; set; }
        public string DataSetsJson { get; }

        public SeleniumRunsViewModel(List<TestRunViewModel> testRunViewModels)
        {
            TestRuns = testRunViewModels;
            Results = TestRuns.Select(r =>
            {
                if (r.IsSeleniumGridErrorRun)
                {
                    return r.Results.First(t => t.IsSeleniumGridError);
                }

                return r.Results.First(t => !t.IsSeleniumGridError);
            }).ToHashSet();
            CalculateLastXBuildsStats(10, LastXBuildStatType.SeleniumGridError);
            
            DataSetsJson = $"[{string.Join(",", GetDatasetsJson())}]";
        }
    }
}