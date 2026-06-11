using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconContentType",
                table: "AppSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconData",
                table: "AppSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "AppSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoData",
                table: "AppSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconContentType",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "IconData",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "LogoData",
                table: "AppSettings");
        }
    }
}
