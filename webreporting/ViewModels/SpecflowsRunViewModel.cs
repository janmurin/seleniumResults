using System.Collections.Generic;
using SeleniumResults.Models;
using SeleniumResults.Models.data;

namespace SeleniumResults.webreporting.ViewModels
{
    public class SpecflowsRunViewModel
    {
        public List<TestRunViewModel> TestRuns { get; set; }
    }
}