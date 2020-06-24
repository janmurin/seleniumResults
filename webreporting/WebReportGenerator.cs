using System;
using System.Collections.Generic;
using System.IO;
using RazorLight;
using SeleniumResults.Models;
using SeleniumResults.ViewModels;

namespace SeleniumResults.webreporting
{
    public static class WebReportGenerator
    {
        private static readonly string TemplateFolderPath = Path.GetFullPath("..\\..\\..\\webreporting\\templates");
        private static readonly string WebreportFolderPath = Path.GetFullPath("..\\..\\..\\webreport");

        public static void GenerateTableHtml(List<SingleTestStats> testStatsList)
        {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(TemplateFolderPath)
                .UseMemoryCachingProvider()
                .Build();

            var viewModel = new TestsPageViewModel()
            {
                TestStatsList = testStatsList
            };

            string result = engine.CompileRenderAsync("testStats.cshtml", viewModel).Result;
            //Console.WriteLine(result);
            File.WriteAllText(Path.Combine(WebreportFolderPath, "tables.html"), result);
        }
    }
}