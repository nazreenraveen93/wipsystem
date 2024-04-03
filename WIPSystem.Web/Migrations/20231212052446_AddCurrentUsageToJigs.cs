using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentUsageToJigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncomingProcesses");

            migrationBuilder.AddColumn<int>(
                name: "CurrentUsage",
                table: "Jigs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentUsage",
                table: "Jigs");

            migrationBuilder.CreateTable(
                name: "IncomingProcesses",
                columns: table => new
                {
                    IncomingProcessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotTravellerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentProcess = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingProcesses", x => x.IncomingProcessId);
                    table.ForeignKey(
                        name: "FK_IncomingProcesses_LotTravellers_LotTravellerId",
                        column: x => x.LotTravellerId,
                        principalTable: "LotTravellers",
                        principalColumn: "LotTravellerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncomingProcesses_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingProcesses_LotTravellerId",
                table: "IncomingProcesses",
                column: "LotTravellerId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingProcesses_ProductId",
                table: "IncomingProcesses",
                column: "ProductId");
        }
    }
}
