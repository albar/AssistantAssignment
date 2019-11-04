using Microsoft.EntityFrameworkCore.Migrations;

namespace Albar.AssistantAssignment.WebApp.Migrations.EvolutionTrackerDatabaseMigrations
{
    public partial class InitialEvolutionTrackerDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RunningTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunningTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chromosomes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Genotype = table.Column<string>(nullable: false),
                    Fitness = table.Column<double>(nullable: false),
                    ObjectiveValues = table.Column<string>(nullable: true),
                    RunningTaskId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chromosomes", x => x.Id);
                    table.UniqueConstraint("AK_Chromosomes_RunningTaskId_Genotype", x => new { x.RunningTaskId, x.Genotype });
                    table.ForeignKey(
                        name: "FK_Chromosomes_RunningTasks_RunningTaskId",
                        column: x => x.RunningTaskId,
                        principalTable: "RunningTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Generations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(nullable: false),
                    RunningTaskId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Generations_RunningTasks_RunningTaskId",
                        column: x => x.RunningTaskId,
                        principalTable: "RunningTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GenerationChromosome",
                columns: table => new
                {
                    GenerationId = table.Column<int>(nullable: false),
                    ChromosomeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationChromosome", x => new { x.ChromosomeId, x.GenerationId });
                    table.ForeignKey(
                        name: "FK_GenerationChromosome_Chromosomes_ChromosomeId",
                        column: x => x.ChromosomeId,
                        principalTable: "Chromosomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenerationChromosome_Generations_GenerationId",
                        column: x => x.GenerationId,
                        principalTable: "Generations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenerationChromosome_GenerationId",
                table: "GenerationChromosome",
                column: "GenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_Generations_RunningTaskId",
                table: "Generations",
                column: "RunningTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenerationChromosome");

            migrationBuilder.DropTable(
                name: "Chromosomes");

            migrationBuilder.DropTable(
                name: "Generations");

            migrationBuilder.DropTable(
                name: "RunningTasks");
        }
    }
}
