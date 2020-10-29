using System;
using System.Collections.Generic;
using SeleniumResults.Models;
using SeleniumResults.Models.enums;

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

        public static HashSet<string> NonExistentTests = new HashSet<string>()
        {
            "BVVFollowUpFromCalendarSmokeTest",
            "BVVFollowUpFromCaseSmokeTest",
            "BVVFollowUpFromEventSmokeTest",
            "BVVFollowUpFromHomeSmokeTest",
            "CARFollowUpFromCalendarSmokeTest",
            "CARFollowUpFromCaseSmokeTest",
            "CARFollowUpFromEventSmokeTest",
            "CARFollowUpFromHomeSmokeTest",
            "EmployeeSmokeTest",
            "EventSmokeTest",
            "PersonListNewAngularSmokeTest",
            "PersonsSmokeTest",
            "ReportOfConcernSmoke",
            "ReportOfConcernFromClientDetailsSmokeTest",
            "PPTFollowUpFromCalendarSmokeTest",
            "PPTFollowUpFromCaseSmokeTest",
            "PPTFollowUpFromEventSmokeTest",
            "PPTFollowUpFromHomeSmokeTest",
            "SCCFollowUpFromCalendarSmokeTest",
            "SCCFollowUpFromCaseSmokeTest",
            "SCCFollowUpFromEventSmokeTest",
            "SCCFollowUpFromHomeSmokeTest",
            "SearchTest",
            "StatisticsSmokeTest"
        };

        public static List<KeyValuePair<string, string>> TestCategoriesDict = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("AdministrationMigrationSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("AdministrationSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("ApiRedirectErrorPageSmokeTest", TestCategories.SCC),
            new KeyValuePair<string, string>("ApprovalSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("BlueLightTest", TestCategories.CAR),
            new KeyValuePair<string, string>("BVAppointmentFromCalendarSmokeTest", TestCategories.BV),
            new KeyValuePair<string, string>("BVAppointmentFromHomeSmokeTest", TestCategories.BV),
            new KeyValuePair<string, string>("BVIncomingPostSmokeTest", TestCategories.BV),
            new KeyValuePair<string, string>("BVVAppointmentFromCalendarSmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("BVVAppointmentFromHomeSmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("BVVIncomingPostSmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("CalendarSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CARAppointmentFromCalendarSmokeTest", TestCategories.CAR),
            new KeyValuePair<string, string>("CARAppointmentFromHomeSmokeTest", TestCategories.CAR),
            new KeyValuePair<string, string>("CaseBackLinkSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CasesSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CheckAssessmentFromCaseSmokeTest", TestCategories.PPT),
            new KeyValuePair<string, string>("CheckAssessmentFromClientDetailsSmokeTest", TestCategories.PPT),
            new KeyValuePair<string, string>("Ck5EditorCorrespondenceSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("Ck5EditorEnquirySmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("Ck5EditorJournalSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CkEditorCorrespondenceSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CkEditorEnquirySmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("CkEditorJournalSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CorrespondenceBackLinkSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CorrespondenceCopyToSiblingsSmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("CorrespondenceCopyToSiblingsSmokeTest", TestCategories.BV),
            new KeyValuePair<string, string>("CorrespondenceFromCaseSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CorrespondenceFromCaseSmokeTest2", TestCategories.BVV),
            new KeyValuePair<string, string>("CorrespondenceFromClientDetailsSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("CorrespondenceTest", TestCategories.All),
            new KeyValuePair<string, string>("CreateTeamTest", TestCategories.All),
            new KeyValuePair<string, string>("DeletePersonSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("EmployeeTest", TestCategories.All),
            new KeyValuePair<string, string>("EnquiryCopyToSiblingsSmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("EnquiryElementsTest", TestCategories.BVV),
            new KeyValuePair<string, string>("EnquiryFromClientDetailsSmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("EnquiryFromEventsSmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("EventBackLinkSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("EventListSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("FollowUpFromCalendarSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("FollowUpFromCaseSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("FollowUpFromEventSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("FollowUpSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("FormattingSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("HomeBackLinkSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("HomeSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("IncomingPostReferralTest", TestCategories.PPT),
            new KeyValuePair<string, string>("IncomingPostSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("InvestigationSmokeTest", TestCategories.BV),
            new KeyValuePair<string, string>("JournalBackLinkSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("JournalCopyToSiblingsSmokeTest", TestCategories.BV),
            new KeyValuePair<string, string>("JournalCopyToSiblingsSmokeTest", TestCategories.BVV),
            new KeyValuePair<string, string>("JournalFromCaseSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("JournalFromClientDetailsSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("KeyfiguresSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("LoginSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("LoginStageSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("MergefieldSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("OrganizationSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("PdfPreviewTest", TestCategories.All),
            new KeyValuePair<string, string>("PersonDetailsTest", TestCategories.All),
            new KeyValuePair<string, string>("PersonListSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("PersonRegisterSmokeTests", TestCategories.All),
            new KeyValuePair<string, string>("PersonsBackLinkSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("PersonModalSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("PersonSearchTest", TestCategories.All),
            new KeyValuePair<string, string>("PostRegistrationOnNewCaseSmokeTest", TestCategories.PPT),
            new KeyValuePair<string, string>("PPTAppointmentFromCalendarSmokeTest", TestCategories.PPT),
            new KeyValuePair<string, string>("PPTAppointmentFromHomeSmokeTest", TestCategories.PPT),
            new KeyValuePair<string, string>("QuerySmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("RolesTest", TestCategories.All),
            new KeyValuePair<string, string>("ReportOfConcernSmokeTest", TestCategories.BV),
            new KeyValuePair<string, string>("SCCAppointmentFromCalendarSmokeTest", TestCategories.SCC),
            new KeyValuePair<string, string>("SCCAppointmentFromHomeSmokeTest", TestCategories.SCC),
            new KeyValuePair<string, string>("SccCk5EditorEnquirySmokeTest", TestCategories.SCC),
            new KeyValuePair<string, string>("SccCkEditorEnquirySmokeTest", TestCategories.SCC),
            new KeyValuePair<string, string>("SccEnquiryFromClientDetailsSmokeTest", TestCategories.SCC),
            new KeyValuePair<string, string>("SccEnquiryFromEventsSmokeTest", TestCategories.SCC),
            new KeyValuePair<string, string>("SignalRBadgeTest", TestCategories.PPT),
            new KeyValuePair<string, string>("StatisticsTableSmokeTest", TestCategories.All),
            new KeyValuePair<string, string>("TemplateEditorTest", TestCategories.All),
            new KeyValuePair<string, string>("TemplatesSmokeTest", TestCategories.All)
        };
    }
}