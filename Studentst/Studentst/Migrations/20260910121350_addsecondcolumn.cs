using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Studentst.Migrations
{
    /// <inheritdoc />
    public partial class addsecondcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "teamId",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    teamName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_teamId",
                table: "Students",
                column: "teamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Team_teamId",
                table: "Students",
                column: "teamId",
                principalTable: "Team",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Team_teamId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Students_teamId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "teamId",
                table: "Students");
        }
    }
}
