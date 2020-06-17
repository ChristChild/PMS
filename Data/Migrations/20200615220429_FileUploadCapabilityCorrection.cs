using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PMS.Data.Migrations
{
    public partial class FileUploadCapabilityCorrection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "FileModels");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "FileModels",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "FileModels");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "FileModels",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
