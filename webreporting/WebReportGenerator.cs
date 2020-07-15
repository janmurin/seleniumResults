using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RazorLight;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;
using SeleniumResults.webreporting.ViewModels;

namespace SeleniumResults.webreporting
{
    public static class WebReportGenerator
    {
        private static readonly string TemplateFolderPath = Path.GetFullPath("..\\..\\..\\webreporting\\templates");
        private static readonly string WebreportFolderPath = Path.GetFullPath("..\\..\\..\\webreport");

        public static void GenerateSeleniumsHtml(List<SingleTestStats> testStatsList)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(TemplateFolderPath)
                .UseMemoryCachingProvider()
                .Build();

            var viewModel = new SeleniumTestsViewModel(testStatsList);

            string result = engine.CompileRenderAsync("seleniumTests.cshtml", viewModel).Result;
            //Console.WriteLine(result);
            File.WriteAllText(Path.Combine(WebreportFolderPath, "seleniumTests.html"), result);

            testStatsList.ForEach(testStats =>
            {
                var testViewModel = new SeleniumTestViewModel(testStats);
                result = engine.CompileRenderAsync("seleniumTest.cshtml", testViewModel).Result;
                File.WriteAllText(Path.Combine(WebreportFolderPath, $"testPages/{testStats.Name}.html"), result); 
            });
        }
        
        public static void GenerateBuildsHtml(List<TestRun> buildList)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(TemplateFolderPath)
                .UseMemoryCachingProvider()
                .Build();

            var viewModel = new SeleniumRunsViewModel()
            {
                TestRuns = buildList
            };

            string result = engine.CompileRenderAsync("seleniumRuns.cshtml", viewModel).Result;
            //Console.WriteLine(result);
            File.WriteAllText(Path.Combine(WebreportFolderPath, "seleniumRuns.html"), result);
        }
    }
}