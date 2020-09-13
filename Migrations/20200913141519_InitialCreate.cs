using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SeleniumResults.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Version = table.Column<int>(nullable: false),
                    TestRunId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Time = table.Column<string>(nullable: true),
                    TestResultType = table.Column<int>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    SubtestsJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestRuns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Version = table.Column<int>(nullable: false),
                    BuildNumber = table.Column<int>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    TestRunType = table.Column<int>(nullable: false),
                    FlytApplicationType = table.Column<int>(nullable: false),
                    LastRun = table.Column<DateTime>(nullable: false),
                    OriginalFileName = table.Column<string>(nullable: true),
                    UniqueId = table.Column<string>(nullable: true),
                    IsProcessed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRuns", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "TestRuns");
        }
    }
}
