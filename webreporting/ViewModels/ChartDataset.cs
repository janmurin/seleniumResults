using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SeleniumResults.Models;

namespace SeleniumResults.webreporting.ViewModels
{
    public class ChartDataset
    {
        public string Label { get; }
        public string Fill { get; }
        public string BorderColor { get; }
        public List<ChartPoint> Data { get; }

        public ChartDataset(string label, bool fill, string borderColor, Dictionary<int, LastXBuildsStat> statsDict)
        {
            Label = label;
            Fill = fill ? "true" : "false";
            BorderColor = borderColor;
            Data = new List<ChartPoint>();
            foreach (var lastXBuildsStat in statsDict.Values.Reverse())
            {
                var time = lastXBuildsStat.GetOrderedTestRuns().First().Time;
                Data.Add(new ChartPoint(time, lastXBuildsStat.FailureRate));
            }
        }

        public override string ToString()
        {
            return $"{{label:\"{Label}\", fill:{Fill}, borderColor:\"{BorderColor}\", data:[{string.Join(",", Data)}]}}";
        }
    }

    public class ChartPoint
    {
        public int Y { get; }
        public string X { get; }

        public ChartPoint(string x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{{ x:\"{X}\", y:{Y}}}";
        }
    }
}