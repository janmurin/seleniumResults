using System;

namespace SeleniumResults
{
    public class Constants
    {
        public const int FAILURE_THRESHOLD = 30;
        public const int RESULTS_THRESHOLD = 10;
        public const string TEST_RESULTS_START_DATE = "2020-04-29";
        public static readonly DateTime TEST_RESULTS_START_DATETIME = DateTime.Parse("2020-04-29");
    }
}