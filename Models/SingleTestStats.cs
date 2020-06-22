using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
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
        public int Sel1Failures => Results.Count(x => x.IsFailed && x.TestRunData.TestRunType == TestRunType.Selenium);
        public int Sel2Failures => Results.Count(x => x.IsFailed && x.TestRunData.TestRunType == TestRunType.Selenium2);
        private int Sel1Count => Results.Count(x => x.TestRunData.TestRunType == TestRunType.Selenium);
        private int Sel2Count => Results.Count(x => x.TestRunData.TestRunType == TestRunType.Selenium2);
        private string MostRecentTime { get; set; }
        public double LastXFailureRate
        {
            get => LastXBuildsDict.First().Value.FailureRate;
        }

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
            string[] array = LastXBuildsDict.Select(x => new String($"{x.Key}-{x.Value.FailureRate * 100:0}")).ToArray();
            return string.Join(" ", array);
        }

        public void CalculateLastXBuildsStats(int buildsInGroup)
        {
            var byBuildNumber = GetResultsOrderedByBuildNumber().ToList();
            LastXBuildsDict = new Dictionary<int, LastXBuildsStat>();
            
            if (byBuildNumber.Count > buildsInGroup)
            {
                for (int i = 0; i < byBuildNumber.Count - buildsInGroup; i++)
                {
                    var take = byBuildNumber.Skip(i).Take(buildsInGroup).ToList();
                    int buildNumber = take.First().Key;
                    
                    LastXBuildsDict.TryAdd(buildNumber, new LastXBuildsStat(take));    
                }
            }
            else
            {
                int buildNumber = byBuildNumber.First().Key;
                var take = byBuildNumber.Take(Math.Min(buildsInGroup, byBuildNumber.Count()));
                
                LastXBuildsDict.TryAdd(buildNumber, new LastXBuildsStat(take));
            }

            LastXBuildsDict = LastXBuildsDict.OrderByDescending(x => x.Key)
                .ToDictionary(x => x.Key, y => y.Value);
        }

        private IOrderedEnumerable<IGrouping<int, SingleTestResult>> GetResultsOrderedByBuildNumber()
        {
            return from result in Results.ToList()
                group result by result.TestRunData.BuildNumber
                into appGroup
                orderby appGroup.Key descending 
                select appGroup;
        }

    }
}