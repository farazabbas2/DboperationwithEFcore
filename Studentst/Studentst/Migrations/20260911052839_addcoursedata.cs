using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Studentst.Migrations
{
    /// <inheritdoc />
    public partial class addcoursedata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "duration",
                table: "course",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "course",
                columns: new[] { "id", "Coursename", "duration", "fees" },
                values: new object[,]
                {
                    { 1, "BCA", "3 years", 0 },
                    { 2, "Biotech", "3 years", 0 },
                    { 3, "F&N", "3 years", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "course",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "course",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "course",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.AlterColumn<int>(
                name: "duration",
                table: "course",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
