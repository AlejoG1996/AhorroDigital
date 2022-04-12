using Microsoft.EntityFrameworkCore.Migrations;

namespace AhorroDigital.API.Migrations
{
    public partial class addtabletypeofsaving : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "typeOfSavings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typeOfSavings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_typeOfSavings_Description",
                table: "typeOfSavings",
                column: "Description",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "typeOfSavings");
        }
    }
}
