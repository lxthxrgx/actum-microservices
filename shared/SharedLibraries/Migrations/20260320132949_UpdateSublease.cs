using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibraries.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSublease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Subleases");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractEndDate2",
                table: "Subleases",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsContinuation",
                table: "Subleases",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsContinuation",
                table: "Subleases");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ContractEndDate2",
                table: "Subleases",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Subleases",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
