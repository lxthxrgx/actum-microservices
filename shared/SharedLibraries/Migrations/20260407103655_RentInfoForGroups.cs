using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibraries.Migrations
{
    /// <inheritdoc />
    public partial class RentInfoForGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RentType",
                table: "Groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GroupRentInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    RentTypeDiscriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    CertNumber = table.Column<string>(type: "text", nullable: true),
                    SeriesCert = table.Column<string>(type: "text", nullable: true),
                    Issued = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    Num = table.Column<string>(type: "text", nullable: true),
                    RentNumber = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Person = table.Column<string>(type: "text", nullable: true),
                    Rnokpp = table.Column<string>(type: "text", nullable: true),
                    Edrpou = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRentInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupRentInfos_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupRentInfos_GroupId",
                table: "GroupRentInfos",
                column: "GroupId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupRentInfos");

            migrationBuilder.DropColumn(
                name: "RentType",
                table: "Groups");
        }
    }
}
