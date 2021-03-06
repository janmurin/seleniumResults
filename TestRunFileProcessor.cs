using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using HtmlAgilityPack;
using SeleniumResults.Models;
using SeleniumResults.Models.data;
using SeleniumResults.Models.enums;

namespace SeleniumResults
{
    public static class TestRunFileProcessor
    {
        public static TestRun ProcessFile(string absolutePath, int idx)
        {
            string fileName = absolutePath.Substring(absolutePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            HtmlDocument htmlDoc = new HtmlDocument {OptionFixNestedTags = true};
            htmlDoc.Load(absolutePath);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                Console.WriteLine($"{idx}. filename: {fileName,35}    PARSE ERROR: {string.Join(",", htmlDoc.ParseErrors.Select(x => x.Reason))}");
                return FixErrorsAndParseTestRun(htmlDoc, fileName, absolutePath);
            }

            return ParseTestRun(htmlDoc, fileName);
        }

        private static TestRun FixErrorsAndParseTestRun(HtmlDocument htmlDoc, string shortName, string fileName)
        {
            Console.WriteLine($"fixing parse errors");
            // build a list of nodes ordered by stream position
            NodePositions pos = new NodePositions(htmlDoc);

            // browse all tags detected as not opened
            foreach (HtmlParseError error in htmlDoc.ParseErrors.Where(e => e.Code == HtmlParseErrorCode.TagNotOpened))
            {
                // find the text node just before this error
                HtmlTextNode last = pos.Nodes.OfType<HtmlTextNode>().LastOrDefault(n => n.StreamPosition < error.StreamPosition);
                if (last != null)
                {
                    // fix the text; reintroduce the broken tag
                    last.Text = error.SourceText.Replace("/", "") + last.Text + error.SourceText;
                }
            }

            var testRun = ParseTestRun(htmlDoc, shortName);
            if (testRun != null)
            {
                StreamWriter outputFile = new StreamWriter(Path.Combine(Program.FIXED_DATA_FOLDER, testRun.TestRunMetaData.OriginalFileName), false);
                htmlDoc.Save(outputFile);
            }

            return testRun;
        }

        private static TestRun ParseTestRun(HtmlDocument htmlDoc, string shortName)
        {
            try
            {
                var applicationType = ParseApplicationType(htmlDoc);
                var lastRun = ParseLastRun(htmlDoc);
                var testRunType = ParseTestRunType(htmlDoc);
                var buildNumber = ParseBuildNumber(htmlDoc);
                var duration = ParseDuration(htmlDoc);

                var testRunMetaData = new TestRunMetaData(shortName, applicationType, lastRun, testRunType, buildNumber, duration);
                var results = ParseTestResults(htmlDoc, testRunMetaData);

                return new TestRun(testRunMetaData, results);
            }
            catch (Exception e)
            {
                Console.WriteLine($"parse error: {e.Message}, file: {shortName}");
            }

            return null;
        }

        private static int ParseDuration(HtmlDocument htmlDoc)
        {
            var durationHtml = htmlDoc.DocumentNode?.SelectSingleNode("//div[@id='modal1']//tr//*[contains(text(),' ms')]//parent::tr/td[2]");

            if (durationHtml == null)
            {
                return 0;
            }

            string buildNumber = durationHtml.InnerText.Substring(0, durationHtml.InnerText.IndexOf("."));

            try
            {
                return int.Parse(buildNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(buildNumber);
                throw;
            }
        }

        private static int ParseBuildNumber(HtmlDocument htmlDoc)
        {
            string resultFileUrl = htmlDoc.DocumentNode?.SelectSingleNode("//div[@id='modal1']//tr//*[contains(text(),'TestResult ')]//parent::tr/td[2]")
                .InnerText;

            string buildNumber = resultFileUrl.Substring(resultFileUrl.LastIndexOf("\\", StringComparison.Ordinal));
            buildNumber = buildNumber
                .Replace("\\1.0.", "")
                .Replace(".xml", "")
                .Replace("-1", "")
                .Replace("-2", "");
            return int.Parse(buildNumber);
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
                .Replace("mai", "May")
                .Replace(".", ":");
            var dateTime = DateTime.Parse(date);
            return dateTime;
        }

        private static FlytApplication ParseApplicationType(HtmlDocument htmlDoc)
        {
            string innerText = htmlDoc.DocumentNode?.SelectSingleNode("//div[@id='modal1']//tr//*[contains(text(),'Machine')]//parent::tr/td[2]").InnerText;
            if (innerText.Contains("FLYTCHILDWEB902") || innerText.Contains("FLYTCHILDWEB401") || innerText.Contains("FLYTCHILDWEB402"))
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
                innerText.Contains("FLYTSCWEB402") || innerText.Contains("FLYTSCWEB903"))
            {
                return FlytApplication.SCC;
            }

            throw new Exception($"unknown machine name: [{innerText}]");
        }

        private static List<TestResult> ParseTestResults(HtmlDocument htmlDoc, TestRunMetaData testRunMetaData)
        {
            var cards = htmlDoc.DocumentNode?.SelectNodes("//div[@class='fixtures']//div[@class='card-panel']");

            List<TestResult> results = new List<TestResult>();

            if (cards != null)
            {
                foreach (var card in cards)
                {
                    results.Add(ParseCard(card, testRunMetaData));
                }
            }

            return results;
        }

        private static TestResult ParseCard(HtmlNode card, TestRunMetaData testRunMetaData)
        {
            var name = card.SelectSingleNode(".//span[@class='fixture-name']").InnerText;
            var testResultType = ParseTestResultType(card);
            var startTimeString = card.SelectSingleNode(".//span[@class='startedAt']").InnerText;
            var endTimeString = card.SelectSingleNode(".//span[@class='endedAt']").InnerText;
            var startDateTime = DateTime.Parse(startTimeString);
            var endDateTime = DateTime.Parse(endTimeString);

            var subtestNodes = card.SelectNodes(".//div[@class='fixture-content']//table//tr");
            List<SubTest> subTests = new List<SubTest>();
            foreach (var node in subtestNodes.Skip(1))
            {
                var subName = node.SelectSingleNode(".//td[@class='test-name']").InnerText;
                TestResultType subResultType = TestResultType.Passed;
                string subMessage = "";

                var failedNode = node.SelectSingleNode(".//td[@class='failed']");
                var skippedNode = node.SelectSingleNode(".//td[@class='skipped']");

                if (failedNode != null)
                {
                    subResultType = TestResultType.Failed;
                    subMessage = node.SelectSingleNode(".//td[@class='failed']//pre").InnerText;
                }
                else if (skippedNode != null)
                {
                    subResultType = TestResultType.Skipped;
                    subMessage = node.SelectSingleNode(".//td[@class='skipped']//pre").InnerText;
                }

                subTests.Add(new SubTest(subName, subResultType, subMessage, subTests.Count));
            }

            return new TestResult(testRunMetaData, name, testResultType, startDateTime, endDateTime, subTests);
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

            if (node.HasClass("unknown"))
            {
                return TestResultType.Unknown;
            }

            if (node.HasClass("inconclusive"))
            {
                return TestResultType.Inconclusive;
            }

            throw new Exception($"test result is undefined for: [{node.OuterHtml}]");
        }
    }
}