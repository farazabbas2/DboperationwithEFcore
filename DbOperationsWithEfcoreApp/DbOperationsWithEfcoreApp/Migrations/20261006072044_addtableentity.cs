using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbOperationsWithEfcoreApp.Migrations
{
    /// <inheritdoc />
    public partial class addtableentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Friendships");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "Friendships",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "Friendships");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Friendships",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
