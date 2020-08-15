using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using SeleniumResults.Models;
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
            if (testRun == null)
            {
                return false;
            }

            if (testRun.TestRunMetaData.TestRunType != TestRunType.Selenium && testRun.TestRunMetaData.TestRunType != TestRunType.Selenium2)
            {
                Console.WriteLine($"DB: ignoring test run {testRun}");
                return false;
            }

            //int failures = testRun.Results.Count(x => x.IsFailed);
            //if (failures > Constants.FAILURE_THRESHOLD)
            if (testRun.HasMidnightErrors || testRun.HasTooManyErrors)
            {
                if (!_midnightErrorRuns.Contains(testRun))
                {
                    _midnightErrorRuns.Add(testRun);
                    return true;
                }

                return false;
            }

            // int results = testRun.Results.Count;
            // if (results < Constants.RESULTS_THRESHOLD)
            // {
            //     Console.WriteLine($"skipping test run [{testRun.TestRunMetaData.OriginalFileName}] because of too few results: {results}");
            //     _tooFewResults.Add($"({testRun.TestRunMetaData.OriginalFileName}, results-{results})");
            //     return;
            // }

            // do not add duplicate test runs
            bool isAdded = _testRuns.TryAdd(testRun.GetId(), testRun);
            DuplicateTestRunsCount += isAdded ? 0 : 1;

            return isAdded;
        }

        public void ProcessData()
        {
            _singleTestStatsDict = new ConcurrentDictionary<string, SingleTestStats>();
            int singleTestTotalDuplicates = 0;
            var get11ThBuildNumber = Get11ThBuildNumber();

            foreach (TestRun testRun in _testRuns.Values)
            {
                testRun.Results.ForEach(sr =>
                {
                    // add only passed or failed tests into statistics
                    if (sr.IsPassedOrFailed)
                    {
                        if (!_singleTestStatsDict.ContainsKey(sr.Name))
                        {
                            _singleTestStatsDict.TryAdd(sr.Name, new SingleTestStats(sr, get11ThBuildNumber));
                        }
                        else
                        {
                            bool added = _singleTestStatsDict[sr.Name].Results.Add(sr);
                            singleTestTotalDuplicates += added ? 0 : 1;
                        }
                    }
                });
            }

            if (singleTestTotalDuplicates > 0)
            {
                throw new Exception($"singleTestTotalDuplicates={singleTestTotalDuplicates}. no duplicates are expected");
            }

            foreach (var sr in _singleTestStatsDict.Values)
            {
                sr.CalculateLastXBuildsStats(10);
            }
        }

        public List<SingleTestStats> GetTestStatsList()
        {
            return _singleTestStatsDict.Values.OrderByDescending(x => x.LastXBuildsDict.First().Value.FailureRate).ToList();
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

        #region Statistics

        public void PrintTooFewAndTooMany()
        {
            Console.WriteLine("Too many failures:");
            Console.WriteLine($"{string.Join(",", _midnightErrorRuns)}");
            Console.WriteLine("Too few results:");
            Console.WriteLine($"{string.Join(",", _tooFewResults)}");
        }

        public void PrintDbStats()
        {
            var mostRecentTime = _singleTestStatsDict.Values.OrderByDescending(x => x.GetMostRecentTime()).ToArray()[0].GetMostRecentTime();
            Console.WriteLine($"most recent time: {mostRecentTime}");
            Console.WriteLine($"DuplicateTestRunsCount: {DuplicateTestRunsCount}");
            Console.WriteLine($"Total test runs: {_testRuns.Count}");
        }

        public void PrintSortedListOfTestRuns()
        {
            _testRuns.Values
                .OrderBy(x => x.GetId())
                .ToList()
                .ForEach(tr => { Console.WriteLine($"{tr}"); });
        }

        public void PrintCountOfSelenium2BuildsPerDay()
        {
            var orderedEnumerable =
                from testRun in _testRuns.Values.Where(x =>
                    x.TestRunMetaData.TestRunType == TestRunType.Selenium2 && x.TestRunMetaData.LastRun >= Constants.TEST_RESULTS_START_DATETIME)
                group testRun by testRun.TestRunMetaData.LastRun.ToString("yyyy-MM-dd")
                into newGroup
                orderby newGroup.Key
                select newGroup;

            orderedEnumerable
                .ToList()
                .ForEach(x =>
                {
                    var appGroups = from result in x.ToList()
                        group result by result.TestRunMetaData.FlytApplicationType
                        into appGroup
                        orderby appGroup.Key
                        select appGroup;

                    Console.Write($"{x.Key},  total counts: {x.Count()}, ");
                    appGroups.ToList().ForEach(t => { Console.Write($"{t.Key}-{t.Count(tt => tt.IsPassed)}/{t.Count()} "); });
                    Console.WriteLine($"");
                });
        }

        public void PrintSuccessRateOfSelenium2BuildsPerDay(int daysPeriod)
        {
            var orderedEnumerable =
                from testRun in _testRuns.Values.Where(x =>
                    x.TestRunMetaData.TestRunType == TestRunType.Selenium2 && x.TestRunMetaData.LastRun >= Constants.TEST_RESULTS_START_DATETIME)
                group testRun by testRun.TestRunMetaData.LastRun.DayOfYear / daysPeriod // every x days
                into newGroup
                orderby newGroup.Key
                select newGroup;

            orderedEnumerable
                .ToList()
                .ForEach(x =>
                {
                    var appGroups = from result in x.ToList()
                        group result by result.TestRunMetaData.FlytApplicationType
                        into appGroup
                        orderby appGroup.Key
                        select appGroup;

                    Console.Write($"{x.Key},  total counts: {x.Count()}, ");
                    appGroups.ToList().ForEach(t => { Console.Write($"{t.Key}-{t.Count(tt => tt.IsPassed)}/{t.Count()} "); });
                    Console.WriteLine($"");
                });
        }

        public void PrintEachTestTotalSuccessRate()
        {
            Console.WriteLine($"{"test:",40} {"Selenium1",12} {"Selenium2",12}");
            var orderedResults = _singleTestStatsDict.Values.OrderByDescending(x => x.Sel1Failures + x.Sel2Failures).ToList();

            foreach (var or in orderedResults)
            {
                Console.WriteLine(or);
            }

            var stats = orderedResults.First(x => x.Name == "CorrespondenceFromCaseSmokeTest2");
            var results = stats.Results
                .Where(x => x.TestRunMetaData.TestRunType == TestRunType.Selenium2)
                .OrderByDescending(x => x.Time).ToList();
            Console.WriteLine($"CorrespondenceFromCaseSmokeTest2:\n {string.Join(",\n", results)}");
        }

        public void PrintEachTestSuccessRateForTheLastXBuilds()
        {
            Console.WriteLine($"{"test:",40} {"P",4}");

            _singleTestStatsDict.Values
                .OrderByDescending(x => x.LastXBuildsDict.First().Value.FailureRate)
                .ToList()
                .ForEach(Console.WriteLine);

            Console.WriteLine($"\nDeletePersonSmokeTest");
            var testStats = _singleTestStatsDict.Values.First(x => x.Name == "DeletePersonSmokeTest");
            foreach (var lastXBuildsStat in testStats.LastXBuildsDict)
            {
                Console.WriteLine($"{lastXBuildsStat.Key} = {lastXBuildsStat.Value}");
                lastXBuildsStat.Value.GetOrderedTestRuns().ForEach(x => { Console.WriteLine($"{x}"); });
            }
        }

        #endregion

        #region private

        #endregion
    }
}