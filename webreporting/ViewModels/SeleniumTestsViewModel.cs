using System.Collections.Generic;
using SeleniumResults.Models;

namespace SeleniumResults.webreporting.ViewModels
{
    public class SeleniumTestsViewModel
    {
        public List<SingleTestStats> TestStatsList { get; }
        public SeleniumTestsViewModel(List<SingleTestStats> testStatsList)
        {
            TestStatsList = testStatsList;
        }
    }
}