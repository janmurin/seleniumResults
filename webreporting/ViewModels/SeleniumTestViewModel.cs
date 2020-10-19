using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.WebEncoders.Testing;
using SeleniumResults.Models;
using SeleniumResults.Models.data;
using SeleniumResults.Models.enums;

namespace SeleniumResults.webreporting.ViewModels
{
    public class SeleniumTestViewModel
    {
        public SingleTestStats TestStats { get; }
        public string DataSetsJson { get; }

        public HashSet<string> existingModals = new HashSet<string>();

        public SeleniumTestViewModel(SingleTestStats stats)
        {
            TestStats = stats;
            DataSetsJson = $"[{string.Join(",", TestStats.GetDatasetsJson())}]";
        }

        public LastXBuildsStat GetLastXBuildDataByTestResult(TestResultViewModel sr, bool forModal)
        {
            if (forModal)
            {
                string id = $"{sr.TestResult.TestRunMetaData.FlytApplicationType}-{sr.TestResult.TestRunMetaData.BuildNumber}";
                if (existingModals.Contains(id))
                {
                    return null;
                }

                existingModals.Add(id);
            }

            switch (sr.TestResult.TestRunMetaData.FlytApplicationType)
            {
                case FlytApplication.BV: return GetLastXBuildStat(TestStats.BVLastXBuildsDict, sr.TestResult.TestRunMetaData.BuildNumber);
                case FlytApplication.BVV: return GetLastXBuildStat(TestStats.BVVLastXBuildsDict, sr.TestResult.TestRunMetaData.BuildNumber);
                case FlytApplication.CAR: return GetLastXBuildStat(TestStats.CARLastXBuildsDict, sr.TestResult.TestRunMetaData.BuildNumber);
                case FlytApplication.PPT: return GetLastXBuildStat(TestStats.PPTLastXBuildsDict, sr.TestResult.TestRunMetaData.BuildNumber);
                case FlytApplication.SCC: return GetLastXBuildStat(TestStats.SCCLastXBuildsDict, sr.TestResult.TestRunMetaData.BuildNumber);
                default: throw new Exception($"unknown application type: {sr.TestResult.TestRunMetaData.FlytApplicationType}");
            }
        }

        private LastXBuildsStat GetLastXBuildStat(Dictionary<int, LastXBuildsStat> lastXBuildsStats, int buildNumber)
        {
            if (lastXBuildsStats.ContainsKey(buildNumber))
            {
                return lastXBuildsStats[buildNumber];
            }

            return lastXBuildsStats.Values.Last();
        }
    }
}