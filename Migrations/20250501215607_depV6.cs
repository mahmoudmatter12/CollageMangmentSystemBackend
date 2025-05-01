using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollageMangmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class depV6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_HDDId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HDDID",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "HDDId",
                table: "Departments",
                newName: "HDDID");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_HDDId",
                table: "Departments",
                newName: "IX_Departments_HDDID");

            migrationBuilder.AlterColumn<Guid>(
                name: "HDDID",
                table: "Departments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentCount",
                table: "Departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_HDDID",
                table: "Departments",
                column: "HDDID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_HDDID",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "StudentCount",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "HDDID",
                table: "Departments",
                newName: "HDDId");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_HDDID",
                table: "Departments",
                newName: "IX_Departments_HDDId");

            migrationBuilder.AlterColumn<Guid>(
                name: "HDDId",
                table: "Departments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "HDDID",
                table: "Departments",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_HDDId",
                table: "Departments",
                column: "HDDId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
