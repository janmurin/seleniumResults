using Microsoft.EntityFrameworkCore.Migrations;

namespace SeleniumResults.Migrations
{
    public partial class ResultDaoRunType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FlytApplicationType",
                table: "TestResults",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TestRunType",
                table: "TestResults",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlytApplicationType",
                table: "TestResults");

            migrationBuilder.DropColumn(
                name: "TestRunType",
                table: "TestResults");
        }
    }
}
