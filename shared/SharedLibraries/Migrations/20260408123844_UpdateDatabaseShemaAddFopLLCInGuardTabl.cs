using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibraries.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseShemaAddFopLLCInGuardTabl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Director",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Counterparties",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Edryofop",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rnokpp",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortNameDirector",
                table: "Counterparties",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Director",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "Edryofop",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "Rnokpp",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "ShortNameDirector",
                table: "Counterparties");
        }
    }
}
