using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;
using SeleniumResults.webreporting;

namespace SeleniumResults
{
    class Program
    {
        private static readonly ResultsDatabase ResultsDatabase = new ResultsDatabase();

        static void Main(string[] args)
        {
            string seleniumFilesDir = "..\\..\\..\\webreport\\data";
            ProcessSeleniumData(seleniumFilesDir);

            string specflowDir = "..\\..\\..\\webreport\\spcdata";
            ProcessSpecflowData(specflowDir);
        }

        private static void ProcessSpecflowData(string specflowDir)
        {
            Console.WriteLine($"loading specflow files from: {specflowDir}");

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
                                // if (testRun.TestRunMetaData.OriginalFileName.StartsWith("API"))
                                // {
                                //     string newName = $"api-{testRun.TestRunMetaData.FlytApplicationType}-{testRun.TestRunMetaData.BuildNumber}-{testRun.TestRunMetaData.LastRun.Ticks/1000}.html";
                                //     newName = newName.Replace("-BV-", "-BVN-");
                                //     File.Move(fileName, Path.Combine("..\\..\\..\\data\\specflow", newName), true);
                                // }
                                // if (testRun.TestRunMetaData.OriginalFileName.StartsWith("specflow"))
                                // {
                                //     string newName = $"spc-{testRun.TestRunMetaData.FlytApplicationType}-{testRun.TestRunMetaData.BuildNumber}-{testRun.TestRunMetaData.LastRun.Ticks/1000}.html";
                                //     newName = newName.Replace("-BV-", "-BVN-");
                                //     File.Move(fileName, Path.Combine("..\\..\\..\\data\\specflow", newName), true);
                                // }
                            }
                            else
                            {
                                string shortName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                                Console.WriteLine($"moving file to data/duplicates folder. filename: {shortName}");
                                File.Move(fileName, Path.Combine("..\\..\\..\\data\\duplicates", shortName), true);
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
                .Where(x=>x.TestRunMetaData.TestRunType ==TestRunType.API)
                .OrderBy(x => x.GetId())
                .ToList();
            Console.WriteLine($"api test runs count {testRuns.Count}");
            // foreach (var specflowRun in testRuns)
            // {
            //     Console.WriteLine(specflowRun);
            // }
            WebReportGenerator.GenerateApiRunsHtml(testRuns);
            
            testRuns = specflowRuns
                .Where(x=>x.TestRunMetaData.TestRunType ==TestRunType.Specflow)
                .OrderBy(x => x.GetId())
                .ToList();
            Console.WriteLine($"specflow test runs count {testRuns.Count}");
            // foreach (var specflowRun in testRuns)
            // {
            //     Console.WriteLine(specflowRun);
            // }
            WebReportGenerator.GenerateSpecflowRunsHtml(testRuns);
        }

        static void ProcessSeleniumData(string sourceDir)
        {
            Console.WriteLine($"loading selenium files from: {sourceDir}");

            string[] fileEntries = Directory.GetFiles(sourceDir);
            var limitedEntries = fileEntries;
            //limitedEntries = fileEntries.Take(300).ToArray();

            Parallel.For(0, limitedEntries.Length,
                index =>
                {
                    var fileName = limitedEntries[index];
                    if (!fileName.EndsWith(".html"))
                    {
                        Console.WriteLine($"skipping parsing file [{fileName}]");
                    }
                    else
                    {
                        try
                        {
                            TestRun testRun = TestRunFileProcessor.ProcessFile(fileName, index);
                            var isAdded = ResultsDatabase.AddTestRunData(testRun);
                            if (!isAdded)
                            {
                                Console.WriteLine($"adding duplicate file to data/duplicates folder. filename: {fileName}");
                                File.Copy(fileName, Path.Combine("..\\..\\..\\data\\duplicates", testRun.TestRunMetaData.OriginalFileName), true);
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