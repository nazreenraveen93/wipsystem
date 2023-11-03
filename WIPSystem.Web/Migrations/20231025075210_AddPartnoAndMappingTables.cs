using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnoAndMappingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partnos",
                columns: table => new
                {
                    PartnoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnoName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Custname = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partnos", x => x.PartnoId);
                });

            migrationBuilder.CreateTable(
                name: "PartnoProcessMappings",
                columns: table => new
                {
                    PartnoProcessMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnoId = table.Column<int>(type: "int", nullable: false),
                    ProcessId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    ProcessCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnoProcessMappings", x => x.PartnoProcessMappingId);
                    table.ForeignKey(
                        name: "FK_PartnoProcessMappings_Partnos_PartnoId",
                        column: x => x.PartnoId,
                        principalTable: "Partnos",
                        principalColumn: "PartnoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnoProcessMappings_Process_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Process",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnoProcessMappings_PartnoId",
                table: "PartnoProcessMappings",
                column: "PartnoId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnoProcessMappings_ProcessId",
                table: "PartnoProcessMappings",
                column: "ProcessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnoProcessMappings");

            migrationBuilder.DropTable(
                name: "Partnos");
        }
    }
}
