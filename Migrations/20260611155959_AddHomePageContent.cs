using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class AddHomePageContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomePageJson",
                table: "AppSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomePageJson",
                table: "AppSettings");
        }
    }
}
