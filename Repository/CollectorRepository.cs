using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository.Models;

namespace SeleniumResults.Repository
{
    public class CollectorRepository
    {
        private HashSet<string> _testRunIds;

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
    }
}