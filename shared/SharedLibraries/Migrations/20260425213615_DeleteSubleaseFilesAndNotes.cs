using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedLibraries.Migrations
{
    /// <inheritdoc />
    public partial class DeleteSubleaseFilesAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubleasesFiles_Subleases_SubleaseId",
                table: "SubleasesFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_SubleasesNotes_Subleases_SubleaseId",
                table: "SubleasesNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubleasesNotes",
                table: "SubleasesNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubleasesFiles",
                table: "SubleasesFiles");

            migrationBuilder.RenameTable(
                name: "SubleasesNotes",
                newName: "SubleaseNotes");

            migrationBuilder.RenameTable(
                name: "SubleasesFiles",
                newName: "SubleaseFiles");

            migrationBuilder.RenameIndex(
                name: "IX_SubleasesNotes_SubleaseId",
                table: "SubleaseNotes",
                newName: "IX_SubleaseNotes_SubleaseId");

            migrationBuilder.RenameIndex(
                name: "IX_SubleasesFiles_SubleaseId",
                table: "SubleaseFiles",
                newName: "IX_SubleaseFiles_SubleaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubleaseNotes",
                table: "SubleaseNotes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubleaseFiles",
                table: "SubleaseFiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubleaseFiles_Subleases_SubleaseId",
                table: "SubleaseFiles",
                column: "SubleaseId",
                principalTable: "Subleases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubleaseNotes_Subleases_SubleaseId",
                table: "SubleaseNotes",
                column: "SubleaseId",
                principalTable: "Subleases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubleaseFiles_Subleases_SubleaseId",
                table: "SubleaseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_SubleaseNotes_Subleases_SubleaseId",
                table: "SubleaseNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubleaseNotes",
                table: "SubleaseNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubleaseFiles",
                table: "SubleaseFiles");

            migrationBuilder.RenameTable(
                name: "SubleaseNotes",
                newName: "SubleasesNotes");

            migrationBuilder.RenameTable(
                name: "SubleaseFiles",
                newName: "SubleasesFiles");

            migrationBuilder.RenameIndex(
                name: "IX_SubleaseNotes_SubleaseId",
                table: "SubleasesNotes",
                newName: "IX_SubleasesNotes_SubleaseId");

            migrationBuilder.RenameIndex(
                name: "IX_SubleaseFiles_SubleaseId",
                table: "SubleasesFiles",
                newName: "IX_SubleasesFiles_SubleaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubleasesNotes",
                table: "SubleasesNotes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubleasesFiles",
                table: "SubleasesFiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubleasesFiles_Subleases_SubleaseId",
                table: "SubleasesFiles",
                column: "SubleaseId",
                principalTable: "Subleases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubleasesNotes_Subleases_SubleaseId",
                table: "SubleasesNotes",
                column: "SubleaseId",
                principalTable: "Subleases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
