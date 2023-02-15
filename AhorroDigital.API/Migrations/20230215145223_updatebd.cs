using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AhorroDigital.API.Migrations
{
    /// <inheritdoc />
    public partial class updatebd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contributes_AspNetUsers_UserAdminId",
                table: "Contributes");

            migrationBuilder.AlterColumn<string>(
                name: "UserAdminId",
                table: "Contributes",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Contributes",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Contributes_AspNetUsers_UserAdminId",
                table: "Contributes",
                column: "UserAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contributes_AspNetUsers_UserAdminId",
                table: "Contributes");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Contributes");

            migrationBuilder.AlterColumn<string>(
                name: "UserAdminId",
                table: "Contributes",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contributes_AspNetUsers_UserAdminId",
                table: "Contributes",
                column: "UserAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
