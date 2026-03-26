using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibraries.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupNameInCounterparty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Counterparties",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Counterparties");
        }
    }
}
