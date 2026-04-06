using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibraries.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGuardDeleteNotesAndFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuardsFiles");

            migrationBuilder.DropTable(
                name: "GuardsNotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuardsFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuardsFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuardsFiles_Guards_GuardId",
                        column: x => x.GuardId,
                        principalTable: "Guards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuardsNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuardsNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuardsNotes_Guards_GuardId",
                        column: x => x.GuardId,
                        principalTable: "Guards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuardsFiles_GuardId",
                table: "GuardsFiles",
                column: "GuardId");

            migrationBuilder.CreateIndex(
                name: "IX_GuardsNotes_GuardId",
                table: "GuardsNotes",
                column: "GuardId");
        }
    }
}
