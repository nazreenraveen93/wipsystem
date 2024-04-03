using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedByAndLastUpdatedIntoSplitLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessId",
                table: "UserEntities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "SplitLot",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SplitLot",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEntities_ProcessId",
                table: "UserEntities",
                column: "ProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserEntities_Process_ProcessId",
                table: "UserEntities",
                column: "ProcessId",
                principalTable: "Process",
                principalColumn: "ProcessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEntities_Process_ProcessId",
                table: "UserEntities");

            migrationBuilder.DropIndex(
                name: "IX_UserEntities_ProcessId",
                table: "UserEntities");

            migrationBuilder.DropColumn(
                name: "ProcessId",
                table: "UserEntities");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "SplitLot");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SplitLot");
        }
    }
}
