using System;
using System.Collections.Generic;
using System.Linq;
using SeleniumResults.Models.enums;

namespace SeleniumResults.Models
{
    public class SingleTestStats
    {
        private readonly int _buildNumber10;
        private bool? _olderThan10builds;
        public string Name { get; }
        public HashSet<TestResultViewModel> Results { get; }

        public SingleTestStats(TestResultViewModel sr, int buildNumber10)
        {
            _buildNumber10 = buildNumber10;
            Name = sr.TestResult.Name;
            Results = new HashSet<TestResultViewModel>() {sr};
        }

        public Dictionary<int, LastXBuildsStat> LastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> BVVLastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> CARLastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> SCCLastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> BVLastXBuildsDict { get; set; }
        public Dictionary<int, LastXBuildsStat> PPTLastXBuildsDict { get; set; }

        public bool IsOlderThan10Builds
        {
            get
            {
                if (!_olderThan10builds.HasValue)
                {
                    _olderThan10builds = Results
                        .OrderByDescending(x => x.TestResult.TestRunMetaData.BuildNumber)
                        .First().TestResult.TestRunMetaData.BuildNumber <= _buildNumber10;
                }

                return _olderThan10builds.Value;
            }
        }

        public int Sel1Failures => Results.Count(x => x.IsFailed && x.TestResult.TestRunMetaData.TestRunType == TestRunType.Selenium);
        public int Sel2Failures => Results.Count(x => x.IsFailed && x.TestResult.TestRunMetaData.TestRunType == TestRunType.Selenium2);
        private int Sel1Count => Results.Count(x => x.TestResult.TestRunMetaData.TestRunType == TestRunType.Selenium);
        private int Sel2Count => Results.Count(x => x.TestResult.TestRunMetaData.TestRunType == TestRunType.Selenium2);
        private string MostRecentTime { get; set; }

        public int Sel1FailureRate => (int) (Sel1Count > 0 ? Decimal.Divide(Sel1Failures, Sel1Count) * 100 : 0);
        public string Sel1Stat => $"{Sel1FailureRate:D2} % ({Sel1Failures}/{Sel1Count})";
        public int Sel2FailureRate => (int) (Sel2Count > 0 ? Decimal.Divide(Sel2Failures, Sel2Count) * 100 : 0);
        public string Sel2Stat => $"{Sel2FailureRate:D2} % ({Sel2Failures}/{Sel2Count})";

        public string TotalFailures => $"{Sel1Failures + Sel2Failures}";
        public string TotalRuns => $"{Sel1Count + Sel2Count}";

        public double AverageDurationWhenPassed
        {
            get
            {
                var passedResults = Results.Where(x => x.IsPassed).ToList();
                if (passedResults.Any())
                {
                    return passedResults.Average(y => y.GetDurationMinutes);
                }

                return 0;
            }
        }

        public string AverageDurationWhenPassedString => $"{AverageDurationWhenPassed,0:0.00} min";

        public string GetMostRecentTime()
        {
            if (MostRecentTime == null)
            {
                if (Results.Any())
                {
                    MostRecentTime = Results.OrderByDescending(x => x.TestResult.Time).First().TestResult.Time;
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

        private Dictionary<int, LastXBuildsStat> CreateLastXBuildDictionary(List<IGrouping<int, TestResultViewModel>> byBuildGrouping, int buildsInGroup)
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