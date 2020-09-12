using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository.Models;

namespace SeleniumResults.Repository
{
    public class CollectorRepository
    {
        private HashSet<string> _testRunIds;
        private ConcurrentDictionary<string, SingleTestStats> _singleTestStatsDict;

        public CollectorRepository()
        {
            _testRunIds = GetTestRunIds();
        }

        private HashSet<string> GetTestRunIds()
        {
            using (var db = new CollectorContext())
            {
                return db.TestRuns.Select(x => x.UniqueId).ToHashSet();
            }
        }

        private List<SingleTestResult> GetAllSingleTestResults()
        {
            using (var db = new CollectorContext())
            {
                return db.TestResults
                    .Include(testResultDao => testResultDao.TestRun)
                    .Select(x => new SingleTestResult(x))
                    .ToList();
            }
        }

        public bool AddTestRun(TestRun testRun)
        {
            if (testRun == null || _testRunIds.Contains(testRun.GetUniqueId()))
            {
                return false;
            }

            using (var db = new CollectorContext())
            {
                var entityEntry = db.TestRuns.Add(new TestRunDao(testRun)).Entity;
                db.SaveChanges();
                _testRunIds.Add(testRun.GetUniqueId());

                var testResultDaos = testRun.Results.Select(x => new TestResultDao(x, entityEntry.Id)).ToList();
                db.TestResults.AddRange(testResultDaos);
                db.SaveChanges();
            }

            return true;
        }

        public string[] GetUnprocessedFilenames(string[] allFilePaths)
        {
            var filenames = allFilePaths.Select(x => x.Substring(x.LastIndexOf(Path.DirectorySeparatorChar) + 1)).ToHashSet();
            using (var db = new CollectorContext())
            {
                var processedTestRuns = db.TestRuns.Where(x => x.IsProcessed).Select(x => x.OriginalFileName).ToHashSet();
                return filenames.Except(processedTestRuns) as string[];
            }
        }

        public void ProcessData()
        {
            _singleTestStatsDict = new ConcurrentDictionary<string, SingleTestStats>();
            var get11ThBuildNumber = Get11ThBuildNumber();

            foreach (var sr in GetAllSingleTestResults())
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
                        _singleTestStatsDict[sr.Name].Results.Add(sr);
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
    }
}