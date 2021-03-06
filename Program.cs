﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SeleniumResults.Models;
using SeleniumResults.Models.data;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository;
using SeleniumResults.webreporting;

namespace SeleniumResults
{
    internal static class Program
    {
        private static readonly CollectorRepository _collectorRepository = new CollectorRepository();
        public static string DATA_FOLDER;
        public static string SPC_DATA_FOLDER;
        public static string FIXED_DATA_FOLDER;
        public static string TEMPLATE_FOLDER;
        public static string WEB_REPORT_FOLDER;
        private static Stopwatch _stopwatch;
        private static Stopwatch _entireStopwatch;

        static void Main(string[] args)
        {
            // PrintSeleniumRunsPerDay();
            // return;
            
            var now = DateTime.Now;
            _stopwatch = new Stopwatch();
            _entireStopwatch = new Stopwatch();
            _stopwatch.Start();
            _entireStopwatch.Start();
            Console.WriteLine($"\nCurrent time: {now.ToString(CultureInfo.InvariantCulture)}");

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var section = configuration.GetSection("Collector");
            DATA_FOLDER = section.GetValue<string>("dataFolder");
            SPC_DATA_FOLDER = section.GetValue<string>("spcDataFolder");
            FIXED_DATA_FOLDER = section.GetValue<string>("fixedDataFolder");
            TEMPLATE_FOLDER = section.GetValue<string>("templateFolderPath");
            WEB_REPORT_FOLDER = section.GetValue<string>("webreportFolderPath");

            // Console.WriteLine($"loaded settings:");
            // Console.WriteLine($"DATA_FOLDER={DATA_FOLDER}");
            // Console.WriteLine($"SPC_DATA_FOLDER={SPC_DATA_FOLDER}");
            // Console.WriteLine($"FIXED_DATA_FOLDER={FIXED_DATA_FOLDER}");
            // Console.WriteLine($"TEMPLATE_FOLDER={TEMPLATE_FOLDER}");
            // Console.WriteLine($"WEB_REPORT_FOLDER={WEB_REPORT_FOLDER}");

            try
            {
                ProcessSeleniumData();
                ProcessSpecflowData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            LogMessage($"elapsed minutes: {_entireStopwatch.Elapsed.Minutes:D2}:{_entireStopwatch.Elapsed.Seconds:D2}");
            //PrintLatestSeleniumReportIgnoreStats(seleniumFilesDir);
            //PrintApiTestTimes();
        }

        private static void PrintApiTestTimes()
        {
            TestRunViewModel model = _collectorRepository.GetTestRun(TestRunType.API, FlytApplication.BVV, 11563);
            
            WebReportGenerator.GenerateApiRunsStatsHtml(model);
        }
        
        private static void PrintSeleniumRunsPerDay()
        {
            var runs = _collectorRepository.GetLastTestRuns(TestRunType.Selenium2);
            var dictionary = runs.GroupBy(x => x.TestRunMetaData.LastRun.DayOfYear / 7)
                .OrderBy(x => x.Key)
                .ToDictionary(k => k.Key, v => v.ToList().Count);
            
            foreach (var keyValuePair in dictionary)
            {
                Console.WriteLine($"{FirstDateOfWeekISO8601(2020,keyValuePair.Key).ToString("d")}: {keyValuePair.Value}");
            }
        }
        
        private static void PrintLatestSeleniumReportIgnoreStats(string seleniumFilesDir)
        {
            // string[] filenames = {"sel-BVN-1.0.10680-171669.html", "sel-BVV-1.0.10680-171673.html", "sel-CAR-1.0.10680-171661.html", "sel-PPT-1.0.10676-171605.html", "sel-SCC-1.0.10680-171674.html"};
            //
            // foreach (var filename in filenames)
            // {
            //     Console.WriteLine($"filename: {filename}");
            //     var bvvRunnableTests = Constants.TestCategoriesDict
            //         .Where(x => x.Value == TestCategories.All || x.Value == GetTestCategoryFromFilename(filename))
            //         .Select(y => y.Key)
            //         .ToHashSet();
            //     TestRun testRun = TestRunFileProcessor.ProcessFile(Path.Combine(seleniumFilesDir, filename), 0);
            //     Dictionary<string, int> ignoredTestsDurations = new Dictionary<string, int>();
            //
            //     foreach (var testResult in testRun.Results.Where(x => x.IsSkipped))
            //     {
            //         if (!bvvRunnableTests.Contains(testResult.Name))
            //         {
            //             ignoredTestsDurations.Add(testResult.Name, testResult.GetDurationSeconds);
            //         }
            //     }
            //
            //     // print each ignored tests and how long it was running
            //     int idx = 1;
            //     foreach (var keyValuePair in ignoredTestsDurations.OrderByDescending(x => x.Value))
            //     {
            //         Console.WriteLine($"{idx,2}. {keyValuePair.Key,45}: {keyValuePair.Value} s");
            //         idx++;
            //     }
            //
            //     Console.WriteLine($"{"Total:",50} {ignoredTestsDurations.Sum(x => x.Value)} s");
            // }
        }

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            // Use first Thursday in January to get first week of the year as
            // it will never be in Week 52/53
            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            // As we're adding days to a date in Week 1,
            // we need to subtract 1 in order to get the right date for week #1
            if (firstWeek == 1)
            {
                weekNum -= 1;
            }

            // Using the first Thursday as starting week ensures that we are starting in the right year
            // then we add number of weeks multiplied with days
            var result = firstThursday.AddDays(weekNum * 7);

            // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
            return result.AddDays(-3);
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

        private static void ProcessSpecflowData()
        {
            LogMessage($"loading api/specflow files from: {SPC_DATA_FOLDER}");
            string[] fileEntries = Directory.GetFiles(SPC_DATA_FOLDER);
            var unprocessedFilenames = _collectorRepository.GetUnprocessedFilenames(fileEntries);

            LogMessage($"unprocessed files count: {unprocessedFilenames.Count()}");
            Parallel.For(0, unprocessedFilenames.Length,
                index =>
                {
                    var fileName = unprocessedFilenames[index];
                    ProcessFile(fileName, index, SPC_DATA_FOLDER);
                });

            if (unprocessedFilenames.Length == 0)
            {
                return;
            }

            var testRuns = _collectorRepository.GetLastTestRuns(TestRunType.API)
                .OrderByDescending(x => x.TestRunMetaData.LastRun)
                .ToList();
            LogMessage($"api test runs count {testRuns.Count}");
            WebReportGenerator.GenerateApiRunsHtml(testRuns);

            testRuns = _collectorRepository.GetLastTestRuns(TestRunType.Specflow)
                .OrderByDescending(x => x.TestRunMetaData.LastRun)
                .ToList();
            LogMessage($"specflow test runs count {testRuns.Count}");
            WebReportGenerator.GenerateSpecflowRunsHtml(testRuns);
        }

        static void ProcessSeleniumData()
        {
            LogMessage($"loading selenium files from: {DATA_FOLDER}");

            string[] filePaths = Directory.GetFiles(DATA_FOLDER);
            //filePaths = filePaths.Take(100).ToArray();
            var unprocessedFilenames = _collectorRepository.GetUnprocessedFilenames(filePaths);

            LogMessage($"unprocessed files count: {unprocessedFilenames.Count()}");
            Parallel.For(0, unprocessedFilenames.Length,
                index =>
                {
                    var fileName = unprocessedFilenames[index];
                    ProcessFile(fileName, index, DATA_FOLDER);
                });

            if (unprocessedFilenames.Length == 0)
            {
                return;
            }

            LogMessage($"processing data");
            _collectorRepository.ProcessData();

            LogMessage("generating selenium test pages");
            WebReportGenerator.GenerateSeleniumTestListHtml(_collectorRepository.GetTestStatsList());
            LogMessage("generating selenium runs page");
            WebReportGenerator.GenerateSeleniumRunsHtml(_collectorRepository.GetLastTestRuns(TestRunType.Selenium2));
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine($"{_stopwatch.Elapsed.Minutes:D2}:{_stopwatch.Elapsed.Seconds:D2} {message}");
            _stopwatch.Restart();
        }

        private static void ProcessFile(string fileName, int index, string folder)
        {
            if (!fileName.EndsWith(".html"))
            {
                Console.WriteLine($"skipping parsing file [{fileName}]");
            }
            else
            {
                var absolutePath = Path.Combine(folder, fileName);
                try
                {
                    TestRun testRun = TestRunFileProcessor.ProcessFile(absolutePath, index);
                    _collectorRepository.AddTestRun(testRun);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"test run was not added into DB. filename: {fileName}. EXCEPTION: {e.Message}. Deleting {absolutePath}");
                    File.Delete(absolutePath);
                }
            }
        }
    }
}