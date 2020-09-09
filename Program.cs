using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository;
using SeleniumResults.webreporting;

namespace SeleniumResults
{
    internal static class Program
    {
        private static readonly ResultsDatabase ResultsDatabase = new ResultsDatabase();
        private static readonly CollectorRepository _collectorRepository = new CollectorRepository();
        public static string DATA_FOLDER;
        public static string SPC_DATA_FOLDER;
        public static string FIXED_DATA_FOLDER;

        static void Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var section = Configuration.GetSection("Collector");
            DATA_FOLDER = section.GetValue<string>("dataFolder");
            SPC_DATA_FOLDER = section.GetValue<string>("spcDataFolder");
            FIXED_DATA_FOLDER = section.GetValue<string>("fixedDataFolder");

            Console.WriteLine($"loaded settings:");
            Console.WriteLine($"DATA_FOLDER={DATA_FOLDER}");
            Console.WriteLine($"SPC_DATA_FOLDER={SPC_DATA_FOLDER}");
            Console.WriteLine($"FIXED_DATA_FOLDER={FIXED_DATA_FOLDER}");
            //string seleniumFilesDir = "..\\..\\..\\webreport\\data";
            //ProcessSeleniumData(seleniumFilesDir);

            //string specflowDir = "..\\..\\..\\webreport\\spcdata";
            //string specflowDir = "..\\..\\..\\data\\errors";
            //ProcessSpecflowData(specflowDir);

            //PrintLatestSeleniumReportIgnoreStats(seleniumFilesDir);
        }

        private static void PrintLatestSeleniumReportIgnoreStats(string seleniumFilesDir)
        {
            string[] filenames = {"sel-BVN-1.0.10680-171669.html", "sel-BVV-1.0.10680-171673.html", "sel-CAR-1.0.10680-171661.html", "sel-PPT-1.0.10676-171605.html", "sel-SCC-1.0.10680-171674.html"};

            foreach (var filename in filenames)
            {
                Console.WriteLine($"filename: {filename}");
                var bvvRunnableTests = Constants.TestCategoriesDict
                    .Where(x => x.Value == TestCategories.All || x.Value == GetTestCategoryFromFilename(filename))
                    .Select(y => y.Key)
                    .ToHashSet();
                TestRun testRun = TestRunFileProcessor.ProcessFile(Path.Combine(seleniumFilesDir, filename), 0);
                Dictionary<string, int> ignoredTestsDurations = new Dictionary<string, int>();

                foreach (var testResult in testRun.Results.Where(x => x.IsSkipped))
                {
                    if (!bvvRunnableTests.Contains(testResult.Name))
                    {
                        ignoredTestsDurations.Add(testResult.Name, testResult.GetDurationSeconds);
                    }
                }

                // print each ignored tests and how long it was running
                int idx = 1;
                foreach (var keyValuePair in ignoredTestsDurations.OrderByDescending(x => x.Value))
                {
                    Console.WriteLine($"{idx,2}. {keyValuePair.Key,45}: {keyValuePair.Value} s");
                    idx++;
                }

                Console.WriteLine($"{"Total:",50} {ignoredTestsDurations.Sum(x => x.Value)} s");
            }
        }

        private static string GetTestCategoryFromFilename(string filename)
        {
            if (filename.Contains("BVN"))
            {
                return TestCategories.BV;
            }

            if (filename.Contains("BVV"))
            {
                return TestCategories.BVV;
            }

            if (filename.Contains("CAR"))
            {
                return TestCategories.CAR;
            }

            if (filename.Contains("PPT"))
            {
                return TestCategories.PPT;
            }

            if (filename.Contains("SCC"))
            {
                return TestCategories.SCC;
            }

            return "error";
        }

        private static void ProcessSpecflowData(string specflowDir)
        {
            Console.WriteLine($"loading api/specflow files from: {specflowDir}");

            string[] fileEntries = Directory.GetFiles(specflowDir);

            ConcurrentBag<TestRun> specflowRuns = new ConcurrentBag<TestRun>();

            Parallel.For(0, fileEntries.Length,
                index =>
                {
                    var fileName = fileEntries[index];
                    if (!fileName.EndsWith(".html"))
                    {
                        Console.WriteLine($"skipping parsing file [{fileName}]");
                    }
                    else
                    {
                        try
                        {
                            TestRun testRun = TestRunFileProcessor.ProcessFile(fileName, index);
                            //Console.WriteLine(testRun);
                            if (testRun != null)
                            {
                                specflowRuns.Add(testRun);
                            }
                            else
                            {
                                // string shortName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                                // Console.WriteLine($"moving file to data/errors folder. filename: {shortName}");
                                // File.Move(fileName, Path.Combine("..\\..\\..\\data\\errors", shortName), true);
                            }

                            // var isAdded = ResultsDatabase.AddTestRunData(testRun);
                            // if (!isAdded)
                            // {
                            //     Console.WriteLine($"adding duplicate file to data/duplicates folder. filename: {fileName}");
                            //     File.Copy(fileName, Path.Combine("..\\..\\..\\data\\duplicates", testRun.TestRunMetaData.OriginalFileName), true);
                            // }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                });

            var testRuns = specflowRuns
                .Where(x => x.TestRunMetaData.TestRunType == TestRunType.API)
                .OrderBy(x => x.GetUniqueId())
                .ToList();
            Console.WriteLine($"api test runs count {testRuns.Count}");
            // foreach (var specflowRun in testRuns)
            // {
            //     Console.WriteLine(specflowRun);
            // }
            WebReportGenerator.GenerateApiRunsHtml(testRuns);

            testRuns = specflowRuns
                .Where(x => x.TestRunMetaData.TestRunType == TestRunType.Specflow)
                .OrderBy(x => x.GetUniqueId())
                .ToList();
            Console.WriteLine($"specflow test runs count {testRuns.Count}");
            // foreach (var specflowRun in testRuns)
            // {
            //     Console.WriteLine(specflowRun);
            // }
            WebReportGenerator.GenerateSpecflowRunsHtml(testRuns);
        }

        static void ProcessSeleniumData()
        {
            Console.WriteLine($"loading selenium files from: {DATA_FOLDER}");

            string[] filePaths = Directory.GetFiles(DATA_FOLDER);
            var unprocessedFilenames = _collectorRepository.GetUnprocessedFilenames(filePaths);

            Parallel.For(0, unprocessedFilenames.Length,
                index =>
                {
                    var fileName = unprocessedFilenames[index];
                    if (!fileName.EndsWith(".html"))
                    {
                        Console.WriteLine($"skipping parsing file [{fileName}]");
                    }
                    else
                    {
                        try
                        {
                            TestRun testRun = TestRunFileProcessor.ProcessFile(fileName, index);
                            var isAdded = _collectorRepository.AddTestRun(testRun);
                            //var isAdded = ResultsDatabase.AddTestRunData(testRun);
                            if (!isAdded)
                            {
                                Console.WriteLine($"test run was not added into DB. filename: {fileName}");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                });

            ResultsDatabase.ProcessData();

            WebReportGenerator.GenerateSeleniumsHtml(ResultsDatabase.GetTestStatsList());
            WebReportGenerator.GenerateBuildsHtml(ResultsDatabase.GetAllTestRuns());
        }
    }
}