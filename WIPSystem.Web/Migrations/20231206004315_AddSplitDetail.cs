using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSplitDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SplitDetails",
                columns: table => new
                {
                    SplitDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SplitLotId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Camber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SplitDetails", x => x.SplitDetailId);
                    table.ForeignKey(
                        name: "FK_SplitDetails_SplitLot_SplitLotId",
                        column: x => x.SplitLotId,
                        principalTable: "SplitLot",
                        principalColumn: "SplitLotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SplitDetails_SplitLotId",
                table: "SplitDetails",
                column: "SplitLotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SplitDetails");
        }
    }
}
