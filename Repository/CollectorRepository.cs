using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using SeleniumResults.Models;
using SeleniumResults.Models.data;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository.Models;

namespace SeleniumResults.Repository
{
    public class CollectorRepository
    {
        private ConcurrentDictionary<string, TestRunDao> _testRunIds;
        private ConcurrentDictionary<string, SingleTestStats> _singleTestStatsDict;
        private const int VERSION = 1;

        public CollectorRepository()
        {
            _testRunIds = GetTestRunIds();
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

            foreach (var sr in GetAllTestResults(TestRunType.Selenium2))
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

            foreach (var sr in _singleTestStatsDict.Values)
            {
                sr.CalculateLastXBuildsStats(10);
            }
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

        public List<TestRunViewModel> GetAllTestRuns(TestRunType type)
        {
            using (var db = new CollectorContext())
            {
                var testRunDaosDict = db.TestRuns
                    .Where(t => t.TestRunType == type)
                    .ToList()
                    .GroupBy(c => c.Id)
                    .ToDictionary(k => k.Key, v =>
                        v.Select(f => f)
                            .Single());

                var testResultViewModelsDict = db.TestResults
                    .Where(t => t.TestRunType == type)
                    .ToList()
                    .GroupBy(resultDao => resultDao.TestRunId)
                    .ToDictionary(k => k.Key, v =>
                        v.Select(f => new TestResultViewModel(new TestResult(f, testRunDaosDict[f.TestRunId])))
                            .ToList());

                return testRunDaosDict
                    .Select(s => new TestRunViewModel(testResultViewModelsDict[s.Key]))
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

        private List<TestResultViewModel> GetAllTestResults(TestRunType testRunType)
        {
            using (var db = new CollectorContext())
            {
                var testRunDaosDict = db.TestRuns
                    .Where(t => t.TestRunType == testRunType)
                    .ToList()
                    .GroupBy(c => c.Id)
                    .ToDictionary(k => k.Key, v => v.Select(f => f).Single());

                return db.TestResults
                    .Where(t => t.TestRunType == testRunType)
                    .Select(x => new TestResultViewModel(new TestResult(x, testRunDaosDict[x.TestRunId])))
                    .ToList();
            }
        }
    }
}