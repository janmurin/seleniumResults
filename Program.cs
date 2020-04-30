using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace SeleniumResults
{
    class Program
    {
        private static readonly ResultData ResultData = new ResultData();
        private const int FAILURE_THRESHOLD = 11;

        static void Main(string[] args)
        {
            string filesDir = "..\\..\\..\\data";

            Console.WriteLine($"files dir: {filesDir}");

            string[] fileEntries = Directory.GetFiles(filesDir);
            var limitedEntries = fileEntries;
            //limitedEntries = fileEntries.Take(30).ToArray();

            for (int i = 0; i < limitedEntries.Length; i++)
            {
                ProcessFile(limitedEntries[i], i);
            }

            Console.WriteLine($"\n FILES WITH MORE THAN {FAILURE_THRESHOLD} FAILURES SKIPPED");
            Console.WriteLine($"skipped duplicate test results count: {ResultData.TotalDuplicates}\n");
            Console.WriteLine($"most recent time: {ResultData.MostRecentTime}");
            Console.WriteLine($"{"test:",40} {"Selenium1",12} {"Selenium2",12}");
            var orderedResults = ResultData.OrderedData;

            foreach (var or in orderedResults)
            {
                Console.WriteLine(or);
            }

            var stats = orderedResults.First(x => x.Name == "JournalFromClientDetailsSmokeTest");
            var results = stats.Results
                .Where(x => x.IsSel2)
                .OrderByDescending(x => x.Time).ToList();
            Console.WriteLine(
                $"JournalFromClientDetailsSmokeTest: {string.Join(",", results)}");
        }

        private static void ProcessFile(string fileName, int idx)
        {
            string shortName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            Console.Write($"{idx}. filename: {shortName,35}");

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.Load(fileName);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                Console.WriteLine($"    PARSE ERROR: {htmlDoc.ParseErrors}");
            }
            else
            {
                var cards = htmlDoc.DocumentNode?.SelectNodes("//div[@class='fixtures']//div[@class='card-panel']");

                List<SeleniumResult> results = new List<SeleniumResult>();

                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        results.Add(ParseCard(card));
                    }
                }

                var failureCount = results.Count(x => x.IsFailure);
                if (failureCount < FAILURE_THRESHOLD)
                {
                    ResultData.AddResults(results);
                }

                Console.WriteLine($" failures: {failureCount}");
            }
        }

        private static SeleniumResult ParseCard(HtmlNode card)
        {
            // C:\TestResults\Selenium\
            SeleniumResult sr = new SeleniumResult();
            sr.IsSel2 = card.SelectSingleNode("//div[@id='modal1']").InnerText.Contains("C:\\TestResults\\Selenium2\\");
            sr.Name = card.SelectSingleNode(".//span[@class='fixture-name']").InnerText;
            sr.IsFailure = card.SelectSingleNode(".//span[contains(@class, 'fixture-result')]").HasClass("failed");
            sr.Time = card.SelectSingleNode(".//span[@class='endedAt']").InnerText;
            //Console.Write($"   test-{sr.Name,40} time-{sr.Time}");

            return sr;
        }
    }
}