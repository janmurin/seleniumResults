using System;
using System.Collections.Generic;
using System.Linq;

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
        public int Sel1Failures => Results.Count(x => x.IsFailed && x.TestRunData.TestRunType == TestRunType.Selenium);
        public int Sel2Failures => Results.Count(x => x.IsFailed && x.TestRunData.TestRunType == TestRunType.Selenium2);
        private int Sel1Count => Results.Count(x => x.TestRunData.TestRunType == TestRunType.Selenium);
        private int Sel2Count => Results.Count(x => x.TestRunData.TestRunType == TestRunType.Selenium2);
        private string MostRecentTime { get; set; }

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
                $"   total: {Sel1Failures + Sel2Failures,2}/{Sel1Count + Sel2Count,3}";
        }
    }
}