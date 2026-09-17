using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbOperationsWithEfcoreApp.Migrations
{
    /// <inheritdoc />
    public partial class chanegcoloumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 1,
                column: "isDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 2,
                column: "isDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 3,
                column: "isDeleted",
                value: true);

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 4,
                column: "isDeleted",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 1,
                column: "isDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 2,
                column: "isDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 3,
                column: "isDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "id",
                keyValue: 4,
                column: "isDeleted",
                value: false);
        }
    }
}
