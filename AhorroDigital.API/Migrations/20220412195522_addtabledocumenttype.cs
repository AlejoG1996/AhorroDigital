using Microsoft.EntityFrameworkCore.Migrations;

namespace AhorroDigital.API.Migrations
{
    public partial class addtabledocumenttype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_documentTypes_Description",
                table: "documentTypes",
                column: "Description",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documentTypes");
        }
    }
}
