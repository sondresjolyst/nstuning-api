using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class AddDynoRunDisplacementAndPressure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AbsolutePressureKpa",
                table: "DynoRuns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplacementCc",
                table: "DynoRuns",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsolutePressureKpa",
                table: "DynoRuns");

            migrationBuilder.DropColumn(
                name: "DisplacementCc",
                table: "DynoRuns");
        }
    }
}
