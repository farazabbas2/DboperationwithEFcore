using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DbOperationsWithEfcoreApp.Migrations
{
    /// <inheritdoc />
    public partial class added1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "currency",
                table: "Currency");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Currency",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "Currency",
                columns: new[] { "id", "Title", "description" },
                values: new object[,]
                {
                    { 1, "INR", "indian inr" },
                    { 2, "Dollar", "dollar" },
                    { 3, "Euro", "euro" },
                    { 4, "Dinar", "dinar" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Currency",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Currency",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Currency",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Currency",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.AlterColumn<int>(
                name: "Title",
                table: "Currency",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "Currency",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
