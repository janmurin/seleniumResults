using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SeleniumResults.Models;

namespace SeleniumResults
{
    class Program
    {
        private static readonly ResultsDatabase ResultsDatabase = new ResultsDatabase();

        static void Main(string[] args)
        {
            string filesDir = "..\\..\\..\\data";
            long totalSize = 0;

            Console.WriteLine($"files dir: {filesDir}");

            string[] fileEntries = Directory.GetFiles(filesDir);
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
                        TestRun testRun = TestRunFileProcessor.ProcessFile(fileName, index);
                        ResultsDatabase.AddTestRunData(testRun);   
                    }
                });
            

            // STATISTICS
            
            // 0. get too few results and too many failures data
            //ResultsDatabase.PrintTooFewAndTooMany();
            
            // 1. get sorted list of test runs
            List<TestRun> listOfTestRuns = ResultsDatabase.GetSortedListOfTestRuns();
            listOfTestRuns.ForEach(tr=>{ Console.WriteLine($"{tr}"); });

            // 2. get count of builds per day
            Console.WriteLine("2. selenium2 count of builds per day (successful/Total)");
            ResultsDatabase.PrintCountOfSelenium2BuildsPerDay();

            // 3. get success rate by each 3 days
            int daysPeriod = 5;
            Console.WriteLine($"3. selenium2 success rate by every {daysPeriod} days");
            ResultsDatabase.PrintSuccessRateOfSelenium2BuildsPerDay(daysPeriod);
            
            // 4. get total success rate by each test
            Console.WriteLine($"4. total success rate by each test");
            ResultsDatabase.PrintEachTestTotalSuccessRate();


        }

       
    }
}