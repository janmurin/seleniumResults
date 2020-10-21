using System;
using System.Collections.Generic;
using System.Linq;
using SeleniumResults.Models.enums;

namespace SeleniumResults.Models
{
    public sealed class SingleTestStats : LastXBuildStatsByApp
    {
        private readonly int _buildNumber10;
        private bool? _olderThan10Builds;
        public string Name { get; }
        public override HashSet<TestResultViewModel> Results { get; set; }

        public SingleTestStats(List<TestResultViewModel> sr, int buildNumber10Th)
        {
            _buildNumber10 = buildNumber10Th;
            Name = sr.First().TestResult.Name;
            Results = sr.ToHashSet();
            CalculateLastXBuildsStats(10, LastXBuildStatType.Failed);
        }

        public bool IsOlderThan10Builds
        {
            get
            {
                if (!_olderThan10Builds.HasValue)
                {
                    _olderThan10Builds = Results
                        .OrderByDescending(x => x.TestResult.TestRunMetaData.BuildNumber)
                        .First().TestResult.TestRunMetaData.BuildNumber <= _buildNumber10;
                }

                return _olderThan10Builds.Value;
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
    }
}