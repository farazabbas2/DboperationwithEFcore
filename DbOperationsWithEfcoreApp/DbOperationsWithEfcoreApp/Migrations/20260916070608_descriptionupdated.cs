using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbOperationsWithEfcoreApp.Migrations
{
    /// <inheritdoc />
    public partial class descriptionupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 1,
                column: "Description",
                value: "all bout hindi");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 2,
                column: "Description",
                value: "all about tamil");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 3,
                column: "Description",
                value: "all about punjabi");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 4,
                column: "Description",
                value: "all about urdu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 1,
                column: "Description",
                value: "Hindi");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 2,
                column: "Description",
                value: "Tamil");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 3,
                column: "Description",
                value: "Punjabi");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 4,
                column: "Description",
                value: "Urdu");
        }
    }
}
