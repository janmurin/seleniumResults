using System;
using System.Collections.Generic;
using System.Linq;
using SeleniumResults.Models.enums;

namespace SeleniumResults.Models
{
    public class SingleTestStats
    {
        public SingleTestStats(SingleTestResult sr)
        {
            Name = sr.Name;
            Results = new HashSet<SingleTestResult>() {sr};
        }

        public string Name { get; }
        public HashSet<SingleTestResult> Results { get; }
        
        public Dictionary<int, LastXBuildsStat> LastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> BVVLastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> CARLastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> SCCLastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> BVLastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> PPTLastXBuildsDict { get; set; }

        public int Sel1Failures => Results.Count(x => x.IsFailed && x.TestRunMetaData.TestRunType == TestRunType.Selenium);
        public int Sel2Failures => Results.Count(x => x.IsFailed && x.TestRunMetaData.TestRunType == TestRunType.Selenium2);
        private int Sel1Count => Results.Count(x => x.TestRunMetaData.TestRunType == TestRunType.Selenium);
        private int Sel2Count => Results.Count(x => x.TestRunMetaData.TestRunType == TestRunType.Selenium2);
        private string MostRecentTime { get; set; }

        public int LastXFailureRate => LastXBuildsDict.First().Value.FailureRate;

        public int Sel1FailureRate => (int) (Sel1Count > 0 ? Decimal.Divide(Sel1Failures, Sel1Count) * 100 : 0);
        public string Sel1Stat => $"{Sel1FailureRate:D2} % ({Sel1Failures}/{Sel1Count})";
        public int Sel2FailureRate => (int) (Sel2Count > 0 ? Decimal.Divide(Sel2Failures, Sel2Count) * 100 : 0);
        public string Sel2Stat => $"{Sel2FailureRate:D2} % ({Sel2Failures}/{Sel2Count})";

        public string TotalFailures => $"{Sel1Failures + Sel2Failures}";
        public string TotalRuns => $"{Sel1Count + Sel2Count}";
        public string LastXFailureRateString => $"{LastXFailureRate:D2} % ";

        public string GetMostRecentTime()
        {
            if (MostRecentTime == null)
            {
                if (Results.Any())
                {
                    MostRecentTime = Results.OrderByDescending(x => x.Time).ToArray()[0].Time;
                }
            }

            return MostRecentTime;
        }

        public override string ToString()
        {
            decimal sel1Perc = Sel1Count > 0 ? Decimal.Divide(Sel1Failures, Sel1Count) : 0;
            decimal sel2Perc = Sel2Count > 0 ? Decimal.Divide(Sel2Failures, Sel2Count) : 0;
            return
                $"{Name,40}: " +
                $"{Sel1Failures,2}/{Sel1Count,3} ({sel1Perc:N2}) " +
                $"{Sel2Failures,2}/{Sel2Count,3} ({sel2Perc:N2}) " +
                $"   total: {Sel1Failures + Sel2Failures,2}/{Sel1Count + Sel2Count,3} " +
                $"   LastXBuilds={LastXBuildsDict?.FirstOrDefault()}" +
                $"   Progress={GetProgress()}";
        }

        private string GetProgress()
        {
            if (LastXBuildsDict == null)
            {
                return "";
            }

            string[] array = LastXBuildsDict.Select(x => new String($"{x.Key}-{x.Value.FailureRate:0}")).ToArray();
            return string.Join(" ", array);
        }

        public void CalculateLastXBuildsStats(int buildsInGroup)
        {
            var byBuildNumber = GetResultsOrderedByBuildNumber().ToList();
            LastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup).OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.BV).ToList();
            BVLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.BVV).ToList();
            BVVLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.CAR).ToList();
            CARLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.PPT).ToList();
            PPTLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);

            byBuildNumber = GetResultsOrderedByBuildNumber(FlytApplication.SCC).ToList();
            SCCLastXBuildsDict = CreateLastXBuildDictionary(byBuildNumber, buildsInGroup)
                .OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);
        }

        private Dictionary<int, LastXBuildsStat> CreateLastXBuildDictionary(List<IGrouping<int, SingleTestResult>> byBuildGrouping, int buildsInGroup)
        {
            var lastXBuildsDict = new Dictionary<int, LastXBuildsStat>();

            if (byBuildGrouping.Count() > buildsInGroup)
            {
                for (int i = 0; i < byBuildGrouping.Count() - buildsInGroup; i++)
                {
                    var take = byBuildGrouping.Skip(i).Take(buildsInGroup).ToList();
                    int buildNumber = take.First().Key;

                    lastXBuildsDict.TryAdd(buildNumber, new LastXBuildsStat(take));
                }
            }
            else
            {
                int buildNumber = byBuildGrouping.First().Key;
                var take = byBuildGrouping.Take(Math.Min(buildsInGroup, byBuildGrouping.Count()));

                lastXBuildsDict.TryAdd(buildNumber, new LastXBuildsStat(take));
            }

            return lastXBuildsDict;
        }

        private IOrderedEnumerable<IGrouping<int, SingleTestResult>> GetResultsOrderedByBuildNumber()
        {
            return from result in Results.ToList()
                group result by result.TestRunMetaData.BuildNumber
                into appGroup
                orderby appGroup.Key descending
                select appGroup;
        }

        private IOrderedEnumerable<IGrouping<int, SingleTestResult>> GetResultsOrderedByBuildNumber(FlytApplication app)
        {
            return from result in Results.Where(x => x.TestRunMetaData.FlytApplicationType == FlytApplication.BV).ToList()
                group result by result.TestRunMetaData.BuildNumber
                into appGroup
                orderby appGroup.Key descending
                select appGroup;
        }
    }
}