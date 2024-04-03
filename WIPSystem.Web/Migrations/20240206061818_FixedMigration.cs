using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "ProcessCode",
            //    table: "ProductProcessMappings");

            migrationBuilder.CreateTable(
                name: "CurrentStatusProcesses",
                columns: table => new
                {
                    CurrentStatusProcessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProcessStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PIC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NextProcess = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentStatusProcesses", x => x.CurrentStatusProcessId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentStatusProcesses");

            migrationBuilder.AddColumn<int>(
                name: "ProcessCode",
                table: "ProductProcessMappings",
                type: "int",
                nullable: false,
                defaultValue: 0);

        }
    }
}
