using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbOperationsWithEfcoreApp.Migrations
{
    /// <inheritdoc />
    public partial class addnewrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "Books");
        }
    }
}
