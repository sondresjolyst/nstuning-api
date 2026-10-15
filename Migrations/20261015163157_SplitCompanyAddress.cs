using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class SplitCompanyAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "AppSettings",
                newName: "StreetAddress");

            migrationBuilder.AddColumn<string>(
                name: "AddressLocality",
                table: "AppSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressRegion",
                table: "AppSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "AppSettings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicEmail",
                table: "AppSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicPhone",
                table: "AppSettings",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            // "Håbakken 7, 4355 Kvernaland" becomes the street, the postal code and the place. An address in
            // another form stays whole in the street, for the admin to split.
            migrationBuilder.Sql("""
                UPDATE "AppSettings"
                SET "PostalCode" = substring("StreetAddress" from ',\s*(\d{4})\s+\S'),
                    "AddressLocality" = btrim(substring("StreetAddress" from ',\s*\d{4}\s+(.+)$')),
                    "StreetAddress" = btrim(substring("StreetAddress" from '^(.*?),\s*\d{4}\s+\S'))
                WHERE "StreetAddress" ~ ',\s*\d{4}\s+\S';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "AppSettings"
                SET "StreetAddress" = concat_ws(', ', nullif("StreetAddress", ''),
                    nullif(concat_ws(' ', nullif("PostalCode", ''), nullif("AddressLocality", '')), ''));
                """);

            migrationBuilder.DropColumn(
                name: "AddressLocality",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "AddressRegion",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PublicEmail",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PublicPhone",
                table: "AppSettings");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "AppSettings",
                newName: "Address");
        }
    }
}
