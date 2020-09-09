using Microsoft.EntityFrameworkCore;
using SeleniumResults.Repository.Models;

namespace SeleniumResults.Repository
{
    public class CollectorContext : DbContext
    {
        public DbSet<TestRunDao> TestRuns { get; set; }
        public DbSet<TestResultDao> TestResults { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=collector.db");
    }
}