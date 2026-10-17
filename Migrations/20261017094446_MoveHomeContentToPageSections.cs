using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class MoveHomeContentToPageSections : Migration
    {
        private const string CopyToPageSections = """
            INSERT INTO "PageSections" ("Key", "Locale", "SectionsJson", "UpdatedAt")
            SELECT 'home', 'en', "HomePageJson", now()
            FROM "AppSettings"
            WHERE "Id" = 1 AND "HomePageJson" IS NOT NULL AND btrim("HomePageJson") <> ''
            ON CONFLICT ("Key", "Locale") DO NOTHING;
            """;

        private const string CopyBackToSettings = """
            UPDATE "AppSettings" AS s
            SET "HomePageJson" = p."SectionsJson"
            FROM "PageSections" AS p
            WHERE s."Id" = 1 AND p."Key" = 'home' AND p."Locale" = 'en';
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SectionsJson = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageSections_Key_Locale",
                table: "PageSections",
                columns: new[] { "Key", "Locale" },
                unique: true);

            // Copies the home page sections as they are into the English home page row. A row that is
            // already there is kept, so running the copy again changes nothing. HomePageJson stays.
            migrationBuilder.Sql(CopyToPageSections);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Puts sections saved after the move back into HomePageJson before the table goes.
            migrationBuilder.Sql(CopyBackToSettings);

            migrationBuilder.DropTable(
                name: "PageSections");
        }
    }
}
