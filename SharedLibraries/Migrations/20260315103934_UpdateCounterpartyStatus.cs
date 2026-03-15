using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibraries.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCounterpartyStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Counterparties\" ALTER COLUMN \"Status\" TYPE integer USING \"Status\"::integer"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Counterparties",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
