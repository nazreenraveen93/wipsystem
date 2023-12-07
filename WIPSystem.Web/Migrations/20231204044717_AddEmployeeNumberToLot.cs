using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WIPSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeNumberToLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeNumber",
                table: "Lots",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeNumber",
                table: "Lots");
        }
    }
}
