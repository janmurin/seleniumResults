using System;
using System.Collections.Generic;
using System.Linq;
using SeleniumResults.Models.enums;
using SeleniumResults.webreporting.ViewModels;

namespace SeleniumResults.Models
{
    public abstract class LastXBuildStatsByApp
    {
        public abstract HashSet<TestResultViewModel> Results { get; set; }

        public Dictionary<int, LastXBuildsStat> LastXBuildsDict { get; private set; }
        public Dictionary<int, LastXBuildsStat> BVVLastXBuildsDict { get; private set; }
        public Dictionary<int, LastXBuildsStat> CARLastXBuildsDict { get; private set; }
        public Dictionary<int, LastXBuildsStat> SCCLastXBuildsDict { get; private set; }
        public Dictionary<int, LastXBuildsStat> BVLastXBuildsDict { get; private set; }
        public Dictionary<int, LastXBuildsStat> PPTLastXBuildsDict { get; private set; }

        public List<ChartDataset> GetDatasetsJson()
        {
            var dataSets = new List<ChartDataset>();
            
            if (BVLastXBuildsDict.Count > 0)
            {
                dataSets.Add(new ChartDataset(FlytApplication.BV.ToString(), false, "rgb(255, 99, 132)", BVLastXBuildsDict));
            }

            if (BVVLastXBuildsDict.Count > 0)
            {
                dataSets.Add(new ChartDataset(FlytApplication.BVV.ToString(), false, "rgb(255, 159, 64)", BVVLastXBuildsDict));
            }

            if (CARLastXBuildsDict.Count > 0)
            {
                dataSets.Add(new ChartDataset(FlytApplication.CAR.ToString(), false, "rgb(153, 102, 255)", CARLastXBuildsDict));
            }

            if (PPTLastXBuildsDict.Count > 0)
            {
                dataSets.Add(new ChartDataset(FlytApplication.PPT.ToString(), false, "rgb(75, 192, 192)", PPTLastXBuildsDict));
            }

            if (SCCLastXBuildsDict.Count > 0)
            {
                dataSets.Add(new ChartDataset(FlytApplication.SCC.ToString(), false, "rgb(54, 162, 235)", SCCLastXBuildsDict));
            }

            return dataSets;
        }
        
        protected void CalculateLastXBuildsStats(int buildsInGroup, LastXBuildStatType lastXBuildStatType)
        {
            var byBuildNumber = GetResultsOrderedByBuildNumber().ToList();
            LastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup, lastXBuildStatType).OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.BV).ToList();
            BVLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup, lastXBuildStatType)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.BVV).ToList();
            BVVLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup, lastXBuildStatType)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.CAR).ToList();
            CARLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup, lastXBuildStatType)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.PPT).ToList();
            PPTLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup, lastXBuildStatType)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.SCC).ToList();
            SCCLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup, lastXBuildStatType)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);
        }

        private Dictionary<int, LastXBuildsStat> CreateLastXBuildDictionary(List<IGrouping<int, TestResultViewModel>> byBuildGrouping, int buildsInGroup, LastXBuildStatType lastXBuildStatType)
        {
            var lastXBuildsDict = new Dictionary<int, LastXBuildsStat>();
            if (byBuildGrouping.Count == 0)
            {
                return lastXBuildsDict;
            }

            if (byBuildGrouping.Count() >= buildsInGroup)
            {
                for (int i = 0; i <= byBuildGrouping.Count() - buildsInGroup; i++)
                {
                    var take = byBuildGrouping.Skip(i).Take(buildsInGroup).ToList();
                    int buildNumber = take.First().Key;

                    lastXBuildsDict.TryAdd(buildNumber, new LastXBuildsStat(take, lastXBuildStatType));
                }
            }
            else
            {
                int buildNumber = byBuildGrouping.First().Key;
                var take = byBuildGrouping.Take(Math.Min(buildsInGroup, byBuildGrouping.Count()));

                lastXBuildsDict.TryAdd(buildNumber, new LastXBuildsStat(take, lastXBuildStatType));
            }

            return lastXBuildsDict;
        }

        private IOrderedEnumerable<IGrouping<int, TestResultViewModel>> GetResultsOrderedByBuildNumber()
        {
            return from result in Results.ToList()
                group result by result.TestResult.TestRunMetaData.BuildNumber
                into appGroup
                orderby appGroup.Key descending
                select appGroup;
        }

        private IOrderedEnumerable<IGrouping<int, TestResultViewModel>> GetResultsOrderedByBuildNumber(FlytApplication app)
        {
            return from result in Results.Where(x => x.TestResult.TestRunMetaData.FlytApplicationType == app).ToList()
                group result by result.TestResult.TestRunMetaData.BuildNumber
                into appGroup
                orderby appGroup.Key descending
                select appGroup;
        }
    }
}