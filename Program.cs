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
            Console.WriteLine("\nReport 0: db stats");
            //ResultsDatabase.PrintTooFewAndTooMany();
            ResultsDatabase.PrintDbStats();
            
            // Console.WriteLine("\nReport 1: Sorted list of all test runs");
            // ResultsDatabase.PrintSortedListOfTestRuns();

            Console.WriteLine("\nReport 2: selenium2 count of builds per day (successful/Total)");
            ResultsDatabase.PrintCountOfSelenium2BuildsPerDay();

            int daysPeriod = 5;
            Console.WriteLine($"\nReport 3: selenium2 success rate by every {daysPeriod} days");
            ResultsDatabase.PrintSuccessRateOfSelenium2BuildsPerDay(daysPeriod);
            
            Console.WriteLine($"\nReport 4: total success rate by each test");
            ResultsDatabase.PrintEachTestTotalSuccessRate();

            int builds = 10;
            Console.WriteLine($"\nReport 5: tests success rate for the last {builds} builds");
            ResultsDatabase.PrintEachTestSuccessRateForTheLastXBuilds(builds);


        }
    }
}