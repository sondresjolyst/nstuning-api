using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyOrgAndVat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrgNumber",
                table: "AppSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "923 202 374");

            migrationBuilder.AddColumn<bool>(
                name: "VatRegistered",
                table: "AppSettings",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgNumber",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "VatRegistered",
                table: "AppSettings");
        }
    }
}
