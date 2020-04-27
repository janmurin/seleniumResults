using System.Collections.Generic;
using System.Linq;

namespace SeleniumResults
{
    public class ResultData
    {
        public int TotalDuplicates { get; set; }
        public Dictionary<string, TestStats> ResultDict = new Dictionary<string, TestStats>();

        public List<TestStats> OrderedData =>
            ResultDict.Values.OrderByDescending(x => x.Sel1Failures + x.Sel2Failures).ToList();

        public void AddResults(List<SeleniumResult> results)
        {
            results.ForEach(sr =>
            {
                if (!ResultDict.ContainsKey(sr.Name))
                {
                    ResultDict.Add(sr.Name, new TestStats(sr));
                }
                else
                {
                    bool added = ResultDict[sr.Name].Results.Add(sr);
                    TotalDuplicates += added ? 0 : 1;
                }
            });
        }
    }
}