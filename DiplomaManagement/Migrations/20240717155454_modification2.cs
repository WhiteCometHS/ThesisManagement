using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaManagement.Migrations
{
    /// <inheritdoc />
    public partial class modification2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PresentationFiles_Theses_ThesisId2",
                table: "PresentationFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Theses_PresentationFiles_PresentationFileId",
                table: "Theses");

            migrationBuilder.DropIndex(
                name: "IX_Theses_PresentationFileId",
                table: "Theses");

            migrationBuilder.DropIndex(
                name: "IX_PresentationFiles_ThesisId2",
                table: "PresentationFiles");

            migrationBuilder.DropColumn(
                name: "PresentationFileId",
                table: "Theses");

            migrationBuilder.DropColumn(
                name: "ThesisId2",
                table: "PresentationFiles");

            migrationBuilder.CreateIndex(
                name: "IX_PresentationFiles_ThesisId",
                table: "PresentationFiles",
                column: "ThesisId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PresentationFiles_Theses_ThesisId",
                table: "PresentationFiles",
                column: "ThesisId",
                principalTable: "Theses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PresentationFiles_Theses_ThesisId",
                table: "PresentationFiles");

            migrationBuilder.DropIndex(
                name: "IX_PresentationFiles_ThesisId",
                table: "PresentationFiles");

            migrationBuilder.AddColumn<int>(
                name: "PresentationFileId",
                table: "Theses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThesisId2",
                table: "PresentationFiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Theses_PresentationFileId",
                table: "Theses",
                column: "PresentationFileId",
                unique: true,
                filter: "[PresentationFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PresentationFiles_ThesisId2",
                table: "PresentationFiles",
                column: "ThesisId2");

            migrationBuilder.AddForeignKey(
                name: "FK_PresentationFiles_Theses_ThesisId2",
                table: "PresentationFiles",
                column: "ThesisId2",
                principalTable: "Theses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Theses_PresentationFiles_PresentationFileId",
                table: "Theses",
                column: "PresentationFileId",
                principalTable: "PresentationFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
