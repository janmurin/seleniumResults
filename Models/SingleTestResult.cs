using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SeleniumResults.Models.enums;
using SeleniumResults.Repository.Models;

namespace SeleniumResults.Models
{
    public class SingleTestResult
    {
        public TestRunMetaData TestRunMetaData { get; }
        public string Name { get; }
        public string Time { get; }
        public TestResultType TestResultType { get; }
        public DateTime EndTime { get; }
        public DateTime StartTime { get; }
        public List<SubTest> SubTests { get; }

        public SingleTestResult(TestRunMetaData metaData, string name, TestResultType type, DateTime start, DateTime end, List<SubTest> subTests)
        {
            TestRunMetaData = metaData;
            Name = name;
            TestResultType = type;
            Time = end.ToString("yyyy-MM-dd HH:mm:ss");
            EndTime = end;
            StartTime = start;
            SubTests = subTests;
            if (metaData.TestRunType == TestRunType.Selenium2)
            {
                IsMidnightError = StartTime.Hour >= 22 &&
                                  subTests.Any(x => x.Message.ToLower() == "OneTimeSetUp: System.NullReferenceException : Object Reference Not Set To An Instance Of An Object.".ToLower());
            }
            else if (metaData.TestRunType == TestRunType.API)
            {
                IsMidnightError = StartTime.Hour >= 22 &&
                                  subTests.Any(x => x.Message
                                                        .ToLower()
                                                        .StartsWith(
                                                            "System.NullReferenceException : Object Reference Not Set To An Instance Of An Object. ->    At Flow.Tests.API.Hooks.Hooks.CreateNewCustomer()"
                                                                .ToLower())
                                                    || x.Message.ToLower()
                                                        .StartsWith(
                                                            "System.Exception : Unable To Create Administrator. Start Date Can Not Be Before Today ->    At Flow.Tests.API.Hooks.Hooks.CreateNewCustomer()"
                                                                .ToLower()));
            }
        }

        public SingleTestResult(TestResultDao resultDao)
        {
            TestRunMetaData = resultDao.TestRun.;
            Name = resultDao.Name;
            TestResultType = resultDao.TestResultType;
            Time = resultDao.Time;
            EndTime = resultDao.EndTime;
            StartTime = resultDao.StartTime;
            SubTests = JsonConvert.DeserializeObject<List<SubTest>>(resultDao.SubtestsJson);
        }

        public bool IsMidnightError { get; set; }
        public bool IsPassed => TestResultType == TestResultType.Passed;
        public bool IsFailed => TestResultType == TestResultType.Failed;
        public bool IsSkipped => TestResultType == TestResultType.Skipped;
        public bool IsPassedOrSkipped => TestResultType == TestResultType.Passed || TestResultType == TestResultType.Skipped;
        public bool IsPassedOrFailed => TestResultType == TestResultType.Failed || TestResultType == TestResultType.Passed;
        public bool IsSel1 => TestRunMetaData.TestRunType == TestRunType.Selenium;
        public bool IsSel2 => TestRunMetaData.TestRunType == TestRunType.Selenium2;
        public double GetDurationMinutes => (EndTime - StartTime).TotalMinutes;
        public string GetDurationMinutesString => $"{GetDurationMinutes,0:0.00} min";
        public int GetDurationSeconds => (int) (EndTime - StartTime).TotalSeconds;

        public override string ToString()
        {
            return $"time-{Time}, file-{TestRunMetaData.OriginalFileName}, result-{TestResultType}, app-{TestRunMetaData.FlytApplicationType}, buildNumber-{TestRunMetaData.BuildNumber}";
        }

        #region equals

        protected bool Equals(SingleTestResult other)
        {
            return Equals(TestRunMetaData, other.TestRunMetaData) && Name == other.Name && Time == other.Time;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SingleTestResult) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TestRunMetaData, Name, Time);
        }

        public static bool operator ==(SingleTestResult left, SingleTestResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SingleTestResult left, SingleTestResult right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}