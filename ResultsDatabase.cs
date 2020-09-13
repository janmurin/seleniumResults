using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using SeleniumResults.Models;
using SeleniumResults.Models.data;
using SeleniumResults.Models.enums;

namespace SeleniumResults
{
    public class ResultsDatabase
    {
        private readonly ConcurrentDictionary<string, TestRun> _testRuns = new ConcurrentDictionary<string, TestRun>();
        private readonly List<string> _tooFewResults = new List<string>();
        private readonly List<TestRun> _midnightErrorRuns = new List<TestRun>();
        private ConcurrentDictionary<string, SingleTestStats> _singleTestStatsDict;

        private int DuplicateTestRunsCount { get; set; }

        public bool AddTestRunData(TestRun testRun)
        {
            // if (testRun == null)
            // {
            //     return false;
            // }
            //
            // if (testRun.TestRunMetaData.TestRunType != TestRunType.Selenium && testRun.TestRunMetaData.TestRunType != TestRunType.Selenium2)
            // {
            //     Console.WriteLine($"DB: ignoring test run {testRun}");
            //     return false;
            // }
            //
            // //int failures = testRun.Results.Count(x => x.IsFailed);
            // //if (failures > Constants.FAILURE_THRESHOLD)
            // if (testRun.HasMidnightErrors || testRun.HasTooManyErrors)
            // {
            //     if (!_midnightErrorRuns.Contains(testRun))
            //     {
            //         _midnightErrorRuns.Add(testRun);
            //         return true;
            //     }
            //
            //     return false;
            // }
            //
            // // int results = testRun.Results.Count;
            // // if (results < Constants.RESULTS_THRESHOLD)
            // // {
            // //     Console.WriteLine($"skipping test run [{testRun.TestRunMetaData.OriginalFileName}] because of too few results: {results}");
            // //     _tooFewResults.Add($"({testRun.TestRunMetaData.OriginalFileName}, results-{results})");
            // //     return;
            // // }
            //
            // // do not add duplicate test runs
            // bool isAdded = _testRuns.TryAdd(testRun.GetUniqueId(), testRun);
            // DuplicateTestRunsCount += isAdded ? 0 : 1;
            //
            // return isAdded;
            return false;
        }

        public void ProcessData()
        {
            // _singleTestStatsDict = new ConcurrentDictionary<string, SingleTestStats>();
            // int singleTestTotalDuplicates = 0;
            // var get11ThBuildNumber = Get11ThBuildNumber();
            //
            // foreach (TestRun testRun in _testRuns.Values)
            // {
            //     testRun.Results.ForEach(sr =>
            //     {
            //         // add only passed or failed tests into statistics
            //         if (sr.IsPassedOrFailed)
            //         {
            //             if (!_singleTestStatsDict.ContainsKey(sr.Name))
            //             {
            //                 _singleTestStatsDict.TryAdd(sr.Name, new SingleTestStats(sr, get11ThBuildNumber));
            //             }
            //             else
            //             {
            //                 bool added = _singleTestStatsDict[sr.Name].Results.Add(sr);
            //                 singleTestTotalDuplicates += added ? 0 : 1;
            //             }
            //         }
            //     });
            // }
            //
            // if (singleTestTotalDuplicates > 0)
            // {
            //     throw new Exception($"singleTestTotalDuplicates={singleTestTotalDuplicates}. no duplicates are expected");
            // }
            //
            // foreach (var sr in _singleTestStatsDict.Values)
            // {
            //     sr.CalculateLastXBuildsStats(10);
            // }
        }

        public List<SingleTestStats> GetTestStatsList()
        {
            var existingTestsSet = Constants.TestCategoriesDict.Select(y => y.Key).ToHashSet();
            var existingTestStats = _singleTestStatsDict.Values.Where(x => existingTestsSet.Contains(x.Name));
            var newTestStats = _singleTestStatsDict.Values.Where(x => !existingTestsSet.Contains(x.Name));
            foreach (var testStat in newTestStats)
            {
                if (!Constants.NonExistentTests.Contains(testStat.Name))
                {
                    Console.WriteLine($"WARNING: test-[{testStat.Name}] does not exist.");
                }
            }

            return existingTestStats.OrderByDescending(x => x.LastXBuildsDict.First().Value.FailureRate).ToList();
        }

        public List<TestRun> GetAllTestRuns()
        {
            var testRuns = _testRuns.Values.ToList();
            testRuns.AddRange(_midnightErrorRuns);
            return testRuns.OrderByDescending(x => x.TestRunMetaData.LastRun).ToList();
        }

        public int Get11ThBuildNumber()
        {
            var lastBuilds = GetAllTestRuns()
                .GroupBy(x => x.TestRunMetaData.BuildNumber)
                .OrderByDescending(x => x.Key)
                .ToList();

            if (lastBuilds.Count > 10)
            {
                return lastBuilds.Skip(10).First().Key;
            }

            return lastBuilds.Last().Key;
        }
    }
}