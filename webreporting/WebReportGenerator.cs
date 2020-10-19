using System.Collections.Generic;
using System.IO;
using RazorLight;
using SeleniumResults.Models;
using SeleniumResults.webreporting.ViewModels;

namespace SeleniumResults.webreporting
{
    public static class WebReportGenerator
    {
        public static void GenerateSeleniumTestListHtml(List<SingleTestStats> testStatsList)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Program.TEMPLATE_FOLDER)
                .UseMemoryCachingProvider()
                .Build();

            var viewModel = new SeleniumTestsViewModel(testStatsList);

            string result = engine.CompileRenderAsync("seleniumTests.cshtml", viewModel).Result;
            //Console.WriteLine(result);
            File.WriteAllText(Path.Combine(Program.WEB_REPORT_FOLDER, "seleniumTests.html"), result);

            testStatsList.ForEach(testStats =>
            {
                var testViewModel = new SeleniumTestViewModel(testStats);
                result = engine.CompileRenderAsync("seleniumTest.cshtml", testViewModel).Result;
                File.WriteAllText(Path.Combine(Program.WEB_REPORT_FOLDER, $"testPages/{testStats.Name}.html"), result); 
            });
        }
        
        public static void GenerateSeleniumRunsHtml(List<TestRunViewModel> buildList)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Program.TEMPLATE_FOLDER)
                .UseMemoryCachingProvider()
                .Build();

            var viewModel = new SeleniumRunsViewModel(buildList);

            string result = engine.CompileRenderAsync("seleniumRuns.cshtml", viewModel).Result;
            //Console.WriteLine(result);
            File.WriteAllText(Path.Combine(Program.WEB_REPORT_FOLDER, "seleniumRuns.html"), result);
        }

        public static void GenerateApiRunsHtml(List<TestRunViewModel> testRuns)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Program.TEMPLATE_FOLDER)
                .UseMemoryCachingProvider()
                .Build();

            var viewModel = new ApiRunsViewModel()
            {
                TestRuns = testRuns
            };

            string result = engine.CompileRenderAsync("apiRuns.cshtml", viewModel).Result;
            //Console.WriteLine(result);
            File.WriteAllText(Path.Combine(Program.WEB_REPORT_FOLDER, "apiRuns.html"), result);
        }
        
        public static void GenerateSpecflowRunsHtml(List<TestRunViewModel> testRuns)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Program.TEMPLATE_FOLDER)
                .UseMemoryCachingProvider()
                .Build();

            var viewModel = new SpecflowsRunViewModel()
            {
                TestRuns = testRuns
            };

            string result = engine.CompileRenderAsync("specflowRuns.cshtml", viewModel).Result;
            //Console.WriteLine(result);
            File.WriteAllText(Path.Combine(Program.WEB_REPORT_FOLDER, "specflowRuns.html"), result);
        }
    }
}