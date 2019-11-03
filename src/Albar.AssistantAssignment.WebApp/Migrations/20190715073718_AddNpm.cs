using Microsoft.EntityFrameworkCore.Migrations;

namespace Albar.AssistantAssignment.WebApp.Migrations
{
    public partial class AddNpm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Npm",
                table: "Assistants",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Npm",
                table: "Assistants");
        }
    }
}
