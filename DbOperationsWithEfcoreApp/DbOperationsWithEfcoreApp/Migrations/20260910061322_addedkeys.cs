using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbOperationsWithEfcoreApp.Migrations
{
    /// <inheritdoc />
    public partial class addedkeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "age",
                table: "Books");

            migrationBuilder.CreateTable(
                name: "BookPrice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Languageid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookPrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookPrice_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookPrice_Currency_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookPrice_Languages_Languageid",
                        column: x => x.Languageid,
                        principalTable: "Languages",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookPrice_BookId",
                table: "BookPrice",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookPrice_CurrencyId",
                table: "BookPrice",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_BookPrice_Languageid",
                table: "BookPrice",
                column: "Languageid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookPrice");

            migrationBuilder.AddColumn<int>(
                name: "age",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
