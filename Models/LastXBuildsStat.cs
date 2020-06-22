using System;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumResults.Models
{
    public class LastXBuildsStat
    {
        public LastXBuildsStat(IEnumerable<IGrouping<int, SingleTestResult>> buildsGroup)
        {
            Builds = buildsGroup;

            int failed = 0;
            double builds = 0;
            Builds.ToList().ForEach(results =>
            {
                foreach (var testResult in results)
                {
                    failed += testResult.IsPassed ? 0 : 1;
                }

                builds += results.Count();
            });

            TotalBuilds = (int) builds;
            TotalFailed = failed;
            FailureRate = failed / builds;
        }

        public double TotalFailed { get; }

        public double FailureRate { get; }

        public int TotalBuilds { get; }

        public IEnumerable<IGrouping<int, SingleTestResult>> Builds { get; }

        public List<SingleTestResult> GetOrderedTestRuns()
        {
            List<SingleTestResult> orderedResults = new List<SingleTestResult>();

            Builds.ToList().ForEach(results => { orderedResults.AddRange(results); });

            return orderedResults.OrderByDescending(x => x.Time).ToList();
        }

        public override string ToString()
        {
            return $"{FailureRate:N2} total: {TotalFailed,2}/{TotalBuilds}";
        }
    }
}