using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using SeleniumResults.Models;

namespace SeleniumResults
{
    public static class TestRunFileProcessor
    {
        public static TestRun ProcessFile(string fileName, int idx)
        {
            string shortName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.Load(fileName);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                Console.WriteLine($"{idx}. filename: {shortName,35}    PARSE ERROR: {htmlDoc.ParseErrors}");
            }
            else
            {
                try
                {
                    var type = ParseApplicationType(htmlDoc);
                    var lastRun = ParseLastRun(htmlDoc);
                    var testRunType = ParseTestRunType(htmlDoc);
                    var testRun = new TestRun(shortName, type, lastRun, new List<SingleTestResult>(), testRunType);
                    testRun.Results = ParseTestResults(htmlDoc, testRun);
                    
                    return testRun;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"parse error: {e.Message}, file: {shortName}");
                }
            }

            return null;
        }

        private static TestRunType ParseTestRunType(HtmlDocument htmlDoc)
        {
            string innerText = htmlDoc.DocumentNode?.SelectSingleNode("//div[@id='modal1']").InnerText;

            if (innerText == null)
            {
                return TestRunType.Undefined;
            }
            if (innerText.Contains("Selenium2"))
            {
                return TestRunType.Selenium2;
            }
            if (innerText.Contains("Selenium"))
            {
                return TestRunType.Selenium;
            }
            if (innerText.Contains("API"))
            {
                return TestRunType.API;
            }
            if (innerText.Contains("SpecFlow"))
            {
                return TestRunType.Specflow;
            }
            
            return TestRunType.Undefined;
        }

        private static DateTime ParseLastRun(HtmlDocument htmlDoc)
        {
            string date = htmlDoc.DocumentNode?.SelectSingleNode("//div[@id='modal1']//tr//*[contains(text(),'Last Run')]//parent::tr/td[2]").InnerText;
            // few corrections
            date = date.Replace("des", "Dec").Replace("feb", "Feb").Replace("jan","Jan").Replace(".",":");
            var dateTime = DateTime.Parse(date);
            return dateTime;
        }

        private static Application ParseApplicationType(HtmlDocument htmlDoc)
        {
            string innerText = htmlDoc.DocumentNode?.SelectSingleNode("//div[@id='modal1']//tr//*[contains(text(),'Machine')]//parent::tr/td[2]").InnerText;
            if (innerText.Contains("FLYTCHILDWEB902") || innerText.Contains("FLYTCHILDWEB401"))
            {
                return Application.BV;
            }
            if (innerText.Contains("FLYTOMSWEB902") || innerText.Contains("FLYTOMSWEB401"))
            {
                return Application.CAR;
            }
            if (innerText.Contains("FLYTBVVWEB902") || innerText.Contains("FLYTBVVWEB901") || innerText.Contains("FLYTBVVWEB401")  || innerText.Contains("FLYTBVVWEB402"))
            {
                return Application.BVV;
            }
            if (innerText.Contains("FLYTPPTWEB902") || innerText.Contains("FLYTPPTWEB901") || innerText.Contains("FLYTPPTWEB401") || innerText.Contains("FLYTPPTWEB402"))
            {
                return Application.PPT;
            }
            if (innerText.Contains("FLYTSCWEB902") || innerText.Contains("FLYTSCWEB401") || innerText.Contains("FLYTSCWEB901") || innerText.Contains("FLYTSCWEB402"))
            {
                return Application.SCC;
            }
            
            throw new Exception($"unknown machine name: [{innerText}]");
        }

        private static List<SingleTestResult> ParseTestResults(HtmlDocument htmlDoc, TestRun testRun)
        {
            var cards = htmlDoc.DocumentNode?.SelectNodes("//div[@class='fixtures']//div[@class='card-panel']");

            List<SingleTestResult> results = new List<SingleTestResult>();

            if (cards != null)
            {
                foreach (var card in cards)
                {
                    results.Add(ParseCard(card, testRun));
                }
            }

            return results;
        }

        private static SingleTestResult ParseCard(HtmlNode card, TestRun testRun)
        {
            SingleTestResult sr = new SingleTestResult();
            sr.OriginalFile = testRun.FileName;
            sr.TestRunType = testRun.TestRunType;
            sr.Name = card.SelectSingleNode(".//span[@class='fixture-name']").InnerText;
            sr.TestResultType = ParseTestResultType(card);
            sr.Time = card.SelectSingleNode(".//span[@class='endedAt']").InnerText;
            //Console.Write($"   test-{sr.Name,40} time-{sr.Time}");

            return sr;
        }

        private static TestResultType ParseTestResultType(HtmlNode card)
        {
            var node = card.SelectSingleNode(".//span[contains(@class, 'fixture-result')]");
            if (node.HasClass("failed"))
            {
                return TestResultType.Failed;
            }
            if (node.HasClass("passed"))
            {
                return TestResultType.Passed;
            }
            if (node.HasClass("skipped"))
            {
                return TestResultType.Skipped;
            }

            return TestResultType.Undefined;
        }
    }
}