using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotTravellerId = table.Column<int>(type: "int", nullable: false),
                    SplitLotNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OriginalQuantity = table.Column<int>(type: "int", nullable: false),
                    Camber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lots_LotTravellers_LotTravellerId",
                        column: x => x.LotTravellerId,
                        principalTable: "LotTravellers",
                        principalColumn: "LotTravellerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lots_LotTravellerId",
                table: "Lots",
                column: "LotTravellerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lots");
        }
    }
}
