using System;

namespace SeleniumResults
{
    public class Constants
    {
        public const int FAILURE_THRESHOLD = 30;
        public const int RESULTS_THRESHOLD = 10;
        public const string TEST_RESULTS_START_DATE = "2020-04-29";
        public static readonly DateTime TEST_RESULTS_START_DATETIME = DateTime.Parse("2020-04-29");
        public static string SELENIUM_RUNS_PAGE = "Selenium runs";
        public static string SELENIUM_TEST_LIST_PAGE = "Selenium test list";
        public static string API_RUNS_PAGE = "Api runs";
        public static string SPECFLOW_RUNS_PAGE = "Specflow runs";
    }
}