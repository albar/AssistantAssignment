using Microsoft.EntityFrameworkCore.Migrations;

namespace Albar.AssistantAssignment.WebApp.Migrations
{
    public partial class AddCodeToSubject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Subjects",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Subjects");
        }
    }
}
