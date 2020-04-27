using System;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumResults
{
    public class TestStats
    {
        public TestStats(SeleniumResult sr)
        {
            Name = sr.Name;
            Results = new HashSet<SeleniumResult>() {sr};
        }

        public string Name { get; set; }
        public HashSet<SeleniumResult> Results { get; set; }
        public int Sel1Failures => Results.Count(x => x.IsFailure && !x.IsSel2);
        public int Sel2Failures => Results.Count(x => x.IsFailure && x.IsSel2);
        public int Sel1Count => Results.Count(x => !x.IsSel2);
        public int Sel2Count => Results.Count(x => x.IsSel2);

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