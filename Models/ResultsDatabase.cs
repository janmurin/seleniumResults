using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SeleniumResults.Models
{
    public class ResultsDatabase
    {
        private int TotalDuplicates { get; set; }
        private readonly ConcurrentDictionary<string, TestRun> _testRuns = new ConcurrentDictionary<string, TestRun>();
        private readonly List<string> _tooFewResults = new List<string>();
        private readonly List<string> _tooManyFailures = new List<string>();

        public void AddTestRunData(TestRun testRun)
        {
            if (testRun == null)
            {
                return;
            }

            if (testRun.TestRunType != TestRunType.Selenium && testRun.TestRunType != TestRunType.Selenium2)
            {
                Console.WriteLine($"DB: ignoring test run {testRun}");
                return;
            }

            int failures = testRun.Results.Count(x => x.IsFailure);
            if (failures > Constants.FAILURE_THRESHOLD)
            {
                //Console.WriteLine($"skipping test run [{testRun.FileName}] because of too many failures: {failures}");
                _tooManyFailures.Add($"({testRun.FileName}, failures-{failures})");
                return;
            }

            int results = testRun.Results.Count;
            if (results < Constants.RESULTS_THRESHOLD)
            {
                //Console.WriteLine($"skipping test run [{testRun.FileName}] because of too few results: {results}");
                _tooFewResults.Add($"({testRun.FileName}, results-{results})");
                return;
            }

            // do not add duplicate test runs
            _testRuns.TryAdd(testRun.GetId(), testRun);
        }

        #region Statistics

        public void PrintTooFewAndTooMany()
        {
            Console.WriteLine("Too many failures:");
            Console.WriteLine($"{string.Join(",", _tooManyFailures)}");
            Console.WriteLine("Too few results:");
            Console.WriteLine($"{string.Join(",", _tooFewResults)}");
        }

        public List<TestRun> GetSortedListOfTestRuns()
        {
            return _testRuns.Values.OrderBy(x => x.GetId()).ToList();
        }

        public void PrintCountOfSelenium2BuildsPerDay()
        {
            var orderedEnumerable =
                from testRun in _testRuns.Values.Where(x => x.TestRunType == TestRunType.Selenium2 && x.LastRun >= Constants.TEST_RESULTS_START_DATETIME)
                group testRun by testRun.LastRun.ToString("yyyy-MM-dd")
                into newGroup
                orderby newGroup.Key
                select newGroup;

            orderedEnumerable
                .ToList()
                .ForEach(x =>
                {
                    var appGroups = from result in x.ToList()
                        group result by result.ApplicationType
                        into appGroup
                        orderby appGroup.Key
                        select appGroup;

                    Console.Write($"{x.Key},  total counts: {x.Count()}, ");
                    appGroups.ToList().ForEach(t => { Console.Write($"{t.Key}-{t.Count(tt => tt.IsSuccessfull)}/{t.Count()} "); });
                    Console.WriteLine($"");
                });
        }

        public void PrintSuccessRateOfSelenium2BuildsPerDay(int daysPeriod)
        {
            var orderedEnumerable =
                from testRun in _testRuns.Values.Where(x => x.TestRunType == TestRunType.Selenium2 && x.LastRun >= Constants.TEST_RESULTS_START_DATETIME)
                group testRun by testRun.LastRun.DayOfYear / daysPeriod // every x days
                into newGroup
                orderby newGroup.Key
                select newGroup;

            orderedEnumerable
                .ToList()
                .ForEach(x =>
                {
                    var appGroups = from result in x.ToList()
                        group result by result.ApplicationType
                        into appGroup
                        orderby appGroup.Key
                        select appGroup;

                    Console.Write($"{x.Key},  total counts: {x.Count()}, ");
                    appGroups.ToList().ForEach(t => { Console.Write($"{t.Key}-{t.Count(tt => tt.IsSuccessfull)}/{t.Count()} "); });
                    Console.WriteLine($"");
                });
        }

        public void PrintEachTestTotalSuccessRate()
        {
            var resultDict = new ConcurrentDictionary<string, TestStats>();

            foreach (TestRun testRun in _testRuns.Values)
            {
                testRun.Results.ForEach(sr =>
                {
                    if (sr.IsPassedOrFailed)
                    {
                        // add only passed or failed tests into statistics
                        if (!resultDict.ContainsKey(sr.Name))
                        {
                            resultDict.TryAdd(sr.Name, new TestStats(sr));
                        }
                        else
                        {
                            bool added = resultDict[sr.Name].Results.Add(sr);
                            TotalDuplicates += added ? 0 : 1;
                        }
                    }
                });
            }

            var mostRecentTime = resultDict.Values.OrderByDescending(x => x.GetMostRecentTime()).ToArray()[0].GetMostRecentTime();

            Console.WriteLine($"\n FILES WITH MORE THAN {Constants.FAILURE_THRESHOLD} FAILURES SKIPPED");
            Console.WriteLine($"skipped duplicate test results count: {TotalDuplicates}\n");
            Console.WriteLine($"most recent time: {mostRecentTime}");
            Console.WriteLine($"{"test:",40} {"Selenium1",12} {"Selenium2",12}");
            var orderedResults = resultDict.Values.OrderByDescending(x => x.Sel1Failures + x.Sel2Failures).ToList();

            foreach (var or in orderedResults)
            {
                Console.WriteLine(or);
            }

            var stats = orderedResults.First(x => x.Name == "DeletePersonSmokeTest");
            var results = stats.Results
                .Where(x => x.TestRunType == TestRunType.Selenium2)
                .OrderByDescending(x => x.Time).ToList();
            Console.WriteLine($"DeletePersonSmokeTest: {string.Join(",\n", results)}");
        }

        #endregion

        #region private

        #endregion
    }
}