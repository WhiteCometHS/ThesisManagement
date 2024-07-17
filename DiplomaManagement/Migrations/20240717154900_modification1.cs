using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaManagement.Migrations
{
    /// <inheritdoc />
    public partial class modification1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "PresentationFiles");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "PresentationFiles",
                newName: "FileType");

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "PresentationFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "PresentationFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ThesisId",
                table: "PresentationFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThesisId2",
                table: "PresentationFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Uploaded",
                table: "PresentationFiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PresentationFiles_Theses_ThesisId2",
                table: "PresentationFiles");

            migrationBuilder.DropIndex(
                name: "IX_PresentationFiles_ThesisId2",
                table: "PresentationFiles");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "PresentationFiles");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "PresentationFiles");

            migrationBuilder.DropColumn(
                name: "ThesisId",
                table: "PresentationFiles");

            migrationBuilder.DropColumn(
                name: "ThesisId2",
                table: "PresentationFiles");

            migrationBuilder.DropColumn(
                name: "Uploaded",
                table: "PresentationFiles");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "PresentationFiles",
                newName: "ContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "PresentationFiles",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
