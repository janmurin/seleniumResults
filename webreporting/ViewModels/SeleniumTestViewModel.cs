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
        public List<ChartDataset> DataSets { get; }
        public string DataSetsJson { get; }

        public HashSet<string> existingModals = new HashSet<string>();

        public SeleniumTestViewModel(SingleTestStats stats)
        {
            TestStats = stats;
            DataSets = new List<ChartDataset>();
            if (TestStats.BVLastXBuildsDict.Count > 0)
            {
                DataSets.Add(new ChartDataset(FlytApplication.BV.ToString(), false, "rgb(255, 99, 132)", TestStats.BVLastXBuildsDict));
            }

            if (TestStats.BVVLastXBuildsDict.Count > 0)
            {
                DataSets.Add(new ChartDataset(FlytApplication.BVV.ToString(), false, "rgb(255, 159, 64)", TestStats.BVVLastXBuildsDict));
            }

            if (TestStats.CARLastXBuildsDict.Count > 0)
            {
                DataSets.Add(new ChartDataset(FlytApplication.CAR.ToString(), false, "rgb(153, 102, 255)", TestStats.CARLastXBuildsDict));
            }

            if (TestStats.PPTLastXBuildsDict.Count > 0)
            {
                DataSets.Add(new ChartDataset(FlytApplication.PPT.ToString(), false, "rgb(75, 192, 192)", TestStats.PPTLastXBuildsDict));
            }

            if (TestStats.SCCLastXBuildsDict.Count > 0)
            {
                DataSets.Add(new ChartDataset(FlytApplication.SCC.ToString(), false, "rgb(54, 162, 235)", TestStats.SCCLastXBuildsDict));
            }

            DataSetsJson = $"[{string.Join(",", DataSets)}]";
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