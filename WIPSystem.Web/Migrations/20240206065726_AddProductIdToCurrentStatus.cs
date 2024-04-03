using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIdToCurrentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "CurrentStatus",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentStatusProcesses_ProductId",
                table: "CurrentStatusProcesses",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentStatus_ProductId",
                table: "CurrentStatus",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentStatus_Products_ProductId",
                table: "CurrentStatus",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentStatusProcesses_Products_ProductId",
                table: "CurrentStatusProcesses",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentStatus_Products_ProductId",
                table: "CurrentStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrentStatusProcesses_Products_ProductId",
                table: "CurrentStatusProcesses");

            migrationBuilder.DropIndex(
                name: "IX_CurrentStatusProcesses_ProductId",
                table: "CurrentStatusProcesses");

            migrationBuilder.DropIndex(
                name: "IX_CurrentStatus_ProductId",
                table: "CurrentStatus");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "CurrentStatus");
        }
    }
}
