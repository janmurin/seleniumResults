using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Enumeration;
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

            HtmlDocument htmlDoc = new HtmlDocument {OptionFixNestedTags = true};
            htmlDoc.Load(fileName);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                Console.WriteLine($"{idx}. filename: {shortName,35}    PARSE ERROR: {htmlDoc.ParseErrors}");
            }
            else
            {
                try
                {
                    var applicationType = ParseApplicationType(htmlDoc);
                    var lastRun = ParseLastRun(htmlDoc);
                    var testRunType = ParseTestRunType(htmlDoc);
                    var buildNumber = ParseBuildNumber(htmlDoc);

                    var testRunMetaData = new TestRunMetaData(shortName, applicationType, lastRun, testRunType, buildNumber);
                    var results = ParseTestResults(htmlDoc, testRunMetaData);

                    return new TestRun(testRunMetaData, results);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"parse error: {e.Message}, file: {shortName}");
                }
            }

            return null;
        }

        private static string ParseBuildNumber(HtmlDocument htmlDoc)
        {
            string resultFileUrl = htmlDoc.DocumentNode?.SelectSingleNode("//div[@id='modal1']//tr//*[contains(text(),'TestResult ')]//parent::tr/td[2]")
                .InnerText;

            string buildNumber = resultFileUrl.Substring(resultFileUrl.LastIndexOf("\\", StringComparison.Ordinal));
            buildNumber = buildNumber
                .Replace("\\1.0.", "")
                .Replace(".xml", "");
            return buildNumber;
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
            date = date
                .Replace("des", "Dec")
                .Replace("feb", "Feb")
                .Replace("jan", "Jan")
                .Replace(".", ":");
            var dateTime = DateTime.Parse(date);
            return dateTime;
        }

        private static FlytApplication ParseApplicationType(HtmlDocument htmlDoc)
        {
            string innerText = htmlDoc.DocumentNode?.SelectSingleNode("//div[@id='modal1']//tr//*[contains(text(),'Machine')]//parent::tr/td[2]").InnerText;
            if (innerText.Contains("FLYTCHILDWEB902") || innerText.Contains("FLYTCHILDWEB401"))
            {
                return FlytApplication.BV;
            }

            if (innerText.Contains("FLYTOMSWEB902") || innerText.Contains("FLYTOMSWEB401"))
            {
                return FlytApplication.CAR;
            }

            if (innerText.Contains("FLYTBVVWEB902") || innerText.Contains("FLYTBVVWEB901") || innerText.Contains("FLYTBVVWEB401") ||
                innerText.Contains("FLYTBVVWEB402"))
            {
                return FlytApplication.BVV;
            }

            if (innerText.Contains("FLYTPPTWEB902") || innerText.Contains("FLYTPPTWEB901") || innerText.Contains("FLYTPPTWEB401") ||
                innerText.Contains("FLYTPPTWEB402"))
            {
                return FlytApplication.PPT;
            }

            if (innerText.Contains("FLYTSCWEB902") || innerText.Contains("FLYTSCWEB401") || innerText.Contains("FLYTSCWEB901") ||
                innerText.Contains("FLYTSCWEB402"))
            {
                return FlytApplication.SCC;
            }

            throw new Exception($"unknown machine name: [{innerText}]");
        }

        private static List<SingleTestResult> ParseTestResults(HtmlDocument htmlDoc, TestRunMetaData testRunMetaData)
        {
            var cards = htmlDoc.DocumentNode?.SelectNodes("//div[@class='fixtures']//div[@class='card-panel']");

            List<SingleTestResult> results = new List<SingleTestResult>();

            if (cards != null)
            {
                foreach (var card in cards)
                {
                    results.Add(ParseCard(card, testRunMetaData));
                }
            }

            return results;
        }

        private static SingleTestResult ParseCard(HtmlNode card, TestRunMetaData testRunMetaData)
        {
            var name = card.SelectSingleNode(".//span[@class='fixture-name']").InnerText;
            var testResultType = ParseTestResultType(card);
            var time = card.SelectSingleNode(".//span[@class='endedAt']").InnerText;

            return new SingleTestResult(testRunMetaData, name, testResultType, time);
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

            throw new Exception($"test result is undefined for: [{node.OuterHtml}]");
        }
    }
}