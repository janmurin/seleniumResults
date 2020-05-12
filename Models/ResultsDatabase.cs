using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumResults.Models
{
    public class ResultsDatabase
    {
        public int TotalDuplicates { get; set; }

        public readonly ConcurrentDictionary<string, TestStats> ResultDict = new ConcurrentDictionary<string, TestStats>();

        public readonly ConcurrentDictionary<string, TestRun> TestRuns = new ConcurrentDictionary<string, TestRun>();

        public List<string> TooFewResults = new List<string>();
        public List<string> TooManyFailures = new List<string>();

        public List<TestStats> OrderedData => ResultDict.Values.OrderByDescending(x => x.Sel1Failures + x.Sel2Failures).ToList();

        public string MostRecentTime
        {
            get { return ResultDict.Values.OrderByDescending(x => x.GetMostRecentTime()).ToArray()[0].GetMostRecentTime(); }
        }

        public void AddResults(List<SingleTestResult> results)
        {
            results.ForEach(sr =>
            {
                if (!ResultDict.ContainsKey(sr.Name))
                {
                    ResultDict.TryAdd(sr.Name, new TestStats(sr));
                }
                else
                {
                    bool added = ResultDict[sr.Name].Results.Add(sr);
                    TotalDuplicates += added ? 0 : 1;
                }
            });
        }

        public void AddTestRunData(TestRun testRun)
        {
            if (testRun == null)
            {
                return;
            }

            int failures = testRun.Results.Count(x => x.IsFailure);
            if (failures > Constants.FAILURE_THRESHOLD)
            {
                //Console.WriteLine($"skipping test run [{testRun.FileName}] because of too many failures: {failures}");
                TooManyFailures.Add($"({testRun.FileName}, failures-{failures})");
                return;
            }

            int results = testRun.Results.Count;
            if (results < Constants.RESULTS_THRESHOLD)
            {
                //Console.WriteLine($"skipping test run [{testRun.FileName}] because of too few results: {results}");
                TooFewResults.Add($"({testRun.FileName}, results-{results})");
                return;
            }

            TestRuns.TryAdd(testRun.GetId(), testRun);
        }

        #region Statistics

        public List<TestRun> GetSortedListOfTestRuns()
        {
            return TestRuns.Values.OrderBy(x => x.GetId()).ToList();
        }

        public void PrintCountOfSelenium2BuildsPerDay()
        {
            var orderedEnumerable = from testRun in TestRuns.Values.Where(x => x.IsSelenium2 && x.LastRun >= Constants.TEST_RESULTS_START_DATETIME)
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
            var orderedEnumerable = from testRun in TestRuns.Values.Where(x => x.IsSelenium2 && x.LastRun >= Constants.TEST_RESULTS_START_DATETIME)
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
        
        #endregion

        public void PrintTooFewAndTooMany()
        {
            Console.WriteLine("Too many failures:");
            Console.WriteLine($"{string.Join(",", TooManyFailures)}");
            Console.WriteLine("Too few results:");
            Console.WriteLine($"{string.Join(",", TooFewResults)}");
        }


    }
}