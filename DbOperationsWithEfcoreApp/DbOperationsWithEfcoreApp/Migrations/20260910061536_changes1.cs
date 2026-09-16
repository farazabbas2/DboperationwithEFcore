using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbOperationsWithEfcoreApp.Migrations
{
    /// <inheritdoc />
    public partial class changes1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookPrice_Languages_Languageid",
                table: "BookPrice");

            migrationBuilder.DropIndex(
                name: "IX_BookPrice_Languageid",
                table: "BookPrice");

            migrationBuilder.DropColumn(
                name: "Languageid",
                table: "BookPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Languageid",
                table: "BookPrice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookPrice_Languageid",
                table: "BookPrice",
                column: "Languageid");

            migrationBuilder.AddForeignKey(
                name: "FK_BookPrice_Languages_Languageid",
                table: "BookPrice",
                column: "Languageid",
                principalTable: "Languages",
                principalColumn: "id");
        }
    }
}
