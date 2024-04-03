using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixedCascadeIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncomingProcesses_Products_ProductId",
                table: "IncomingProcesses");

            migrationBuilder.AddForeignKey(
             name: "FK_IncomingProcesses_Products_ProductId",
             table: "IncomingProcesses",
             column: "ProductId",
             principalTable: "Products",
             principalColumn: "ProductId",
             onDelete: ReferentialAction.NoAction); // Set to NoAction

        }

    }
}
