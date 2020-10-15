using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SeleniumResults.Models;
using SeleniumResults.Models.data;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository.Models;

namespace SeleniumResults.Repository
{
    public class CollectorRepository
    {
        private readonly ConcurrentDictionary<string, TestRunDao> _testRunIds;
        private ConcurrentDictionary<string, SingleTestStats> _singleTestStatsDict;
        private readonly DateTime _twoMonthsAgo;
        private const int VERSION = 1;

        public CollectorRepository()
        {
            _testRunIds = GetTestRunIds();
            _twoMonthsAgo = DateTime.Now - TimeSpan.FromDays(30);
        }

        public void AddTestRun(TestRun testRun)
        {
            if (testRun == null)
            {
                throw new Exception($"testRun is null");
            }

            if (_testRunIds.ContainsKey(testRun.GetUniqueId()))
            {
                throw new Exception($"not unique with {_testRunIds[testRun.GetUniqueId()].OriginalFileName}");
            }

            using (var db = new CollectorContext())
            {
                var entityEntry = db.TestRuns.Add(new TestRunDao(testRun, VERSION)).Entity;
                db.SaveChanges();
                _testRunIds.TryAdd(testRun.GetUniqueId(), entityEntry);

                var testResultDaos = testRun.Results.Select(x => new TestResultDao(x, entityEntry.Id, VERSION)).ToList();
                db.TestResults.AddRange(testResultDaos);
                db.SaveChanges();
            }
        }

        public string[] GetUnprocessedFilenames(string[] allFilePaths)
        {
            var filenames = allFilePaths.Select(x => x.Substring(x.LastIndexOf(Path.DirectorySeparatorChar) + 1)).ToHashSet();
            using (var db = new CollectorContext())
            {
                var processedTestRuns = db.TestRuns.Select(x => x.OriginalFileName).ToHashSet();
                return filenames.Except(processedTestRuns).ToArray();
            }
        }

        public void ProcessData()
        {
            _singleTestStatsDict = new ConcurrentDictionary<string, SingleTestStats>();
            var get11ThBuildNumber = Get11ThBuildNumber();

            foreach (var sr in GetFilteredTestResults(TestRunType.Selenium2))
            {
                // add only passed or failed tests into statistics
                if (sr.IsPassedOrFailed)
                {
                    if (!_singleTestStatsDict.ContainsKey(sr.TestResult.Name))
                    {
                        _singleTestStatsDict.TryAdd(sr.TestResult.Name, new SingleTestStats(sr, get11ThBuildNumber));
                    }
                    else
                    {
                        _singleTestStatsDict[sr.TestResult.Name].Results.Add(sr);
                    }
                }
            }

            Parallel.ForEach(_singleTestStatsDict.Values, sr =>
            {
                sr.CalculateLastXBuildsStats(10);
            });
        }

        public int Get11ThBuildNumber()
        {
            using (var db = new CollectorContext())
            {
                var lastBuilds = db.TestRuns
                    .ToList()
                    .GroupBy(x => x.BuildNumber)
                    .OrderByDescending(x => x.Key)
                    .ToList();

                if (lastBuilds.Count > 10)
                {
                    return lastBuilds.Skip(10).First().Key;
                }

                return lastBuilds.Last().Key;
            }
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

        public List<TestRunViewModel> GetLastTestRuns(TestRunType type)
        {
            using (var db = new CollectorContext())
            {
                var testResultViewModels = from testResult in db.TestResults
                    join testRun in db.TestRuns on testResult.TestRunId equals testRun.Id
                    where testRun.TestRunType == type && testRun.LastRun > _twoMonthsAgo
                    select new TestResultViewModel(new TestResult(testResult, testRun));
                
                return testResultViewModels
                    .ToList()
                    .GroupBy(t => t.TestResult.TestRunMetaData.Id)
                    .Select(grp => new TestRunViewModel(grp.ToList()))
                    .ToList();
            }
        }

        private ConcurrentDictionary<string, TestRunDao> GetTestRunIds()
        {
            using (var db = new CollectorContext())
            {
                var testRunDaos = db.TestRuns
                    .ToList()
                    .GroupBy(x => x.UniqueId)
                    .ToDictionary(k => k.Key, v => v.Select(f => f).Single());

                return new ConcurrentDictionary<string, TestRunDao>(testRunDaos);
            }
        }

        private List<TestResultViewModel> GetFilteredTestResults(TestRunType testRunType)
        {
            return GetLastTestRuns(testRunType)
                .Where(t => !t.HasMidnightErrors && !t.HasTooManyErrors && !t.HasSeleniumGridErrors)
                .SelectMany(s => s.Results)
                .ToList();
        }
    }
}