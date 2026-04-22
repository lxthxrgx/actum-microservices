using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibraries.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCounterpartyModelAddRnokppToCounterpartyModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CounterpartyFop_Rnokpp",
                table: "Counterparties");

            migrationBuilder.AlterColumn<string>(
                name: "Rnokpp",
                table: "Counterparties",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Rnokpp",
                table: "Counterparties",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CounterpartyFop_Rnokpp",
                table: "Counterparties",
                type: "text",
                nullable: true);
        }
    }
}
