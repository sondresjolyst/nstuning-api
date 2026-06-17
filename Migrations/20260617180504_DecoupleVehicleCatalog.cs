using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleVehicleCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // New columns and join table first; data migration below relies on the old
            // CarVariants table + CarEngines.VariantId still being present.
            migrationBuilder.AddColumn<string>(
                name: "Family",
                table: "CarModels",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "CarEngines",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarModelEngines",
                columns: table => new
                {
                    ModelId = table.Column<int>(type: "integer", nullable: false),
                    EngineId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModelEngines", x => new { x.ModelId, x.EngineId });
                    table.ForeignKey(
                        name: "FK_CarModelEngines_CarEngines_EngineId",
                        column: x => x.EngineId,
                        principalTable: "CarEngines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarModelEngines_CarModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "CarModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // --- Data migration (decouple engines from variants) ---

            // 1. Tag each existing engine with the brand it belonged to via variant -> model -> brand.
            migrationBuilder.Sql(@"
                UPDATE ""CarEngines"" AS e
                SET ""BrandId"" = m.""BrandId""
                FROM ""CarVariants"" AS v
                JOIN ""CarModels"" AS m ON m.""Id"" = v.""ModelId""
                WHERE e.""VariantId"" = v.""Id"";");

            // 2. Backfill factory-fitment links from the old ownership chain, mapping each
            //    engine to its survivor (lowest Id per brand + case-insensitive name).
            migrationBuilder.Sql(@"
                INSERT INTO ""CarModelEngines"" (""ModelId"", ""EngineId"")
                SELECT DISTINCT v.""ModelId"", s.""SurvivorId""
                FROM ""CarEngines"" AS e
                JOIN ""CarVariants"" AS v ON v.""Id"" = e.""VariantId""
                JOIN (
                    SELECT ""Id"",
                           MIN(""Id"") OVER (PARTITION BY ""BrandId"", lower(""Name"")) AS ""SurvivorId""
                    FROM ""CarEngines""
                ) AS s ON s.""Id"" = e.""Id"";");

            // 3. Collapse duplicate engines created by the old per-variant ownership.
            migrationBuilder.Sql(@"
                DELETE FROM ""CarEngines"" AS e
                USING (
                    SELECT ""Id"",
                           MIN(""Id"") OVER (PARTITION BY ""BrandId"", lower(""Name"")) AS ""SurvivorId""
                    FROM ""CarEngines""
                ) AS s
                WHERE s.""Id"" = e.""Id"" AND s.""Id"" <> s.""SurvivorId"";");

            // --- Drop the old engine -> variant coupling ---
            migrationBuilder.DropForeignKey(
                name: "FK_CarEngines_CarVariants_VariantId",
                table: "CarEngines");

            migrationBuilder.DropIndex(
                name: "IX_CarEngines_VariantId_Name",
                table: "CarEngines");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "CarEngines");

            // --- New indexes / FKs for the decoupled catalog ---
            migrationBuilder.CreateIndex(
                name: "IX_CarEngines_BrandId_Name",
                table: "CarEngines",
                columns: new[] { "BrandId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarModelEngines_EngineId",
                table: "CarModelEngines",
                column: "EngineId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarEngines_CarBrands_BrandId",
                table: "CarEngines",
                column: "BrandId",
                principalTable: "CarBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Structural reverse only — the original engine -> variant ownership cannot be
            // reconstructed (VariantId was dropped). Forward-only in practice.
            migrationBuilder.DropForeignKey(
                name: "FK_CarEngines_CarBrands_BrandId",
                table: "CarEngines");

            migrationBuilder.DropIndex(
                name: "IX_CarEngines_BrandId_Name",
                table: "CarEngines");

            migrationBuilder.DropTable(
                name: "CarModelEngines");

            migrationBuilder.DropColumn(
                name: "Family",
                table: "CarModels");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "CarEngines");

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "CarEngines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CarEngines_VariantId_Name",
                table: "CarEngines",
                columns: new[] { "VariantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CarEngines_CarVariants_VariantId",
                table: "CarEngines",
                column: "VariantId",
                principalTable: "CarVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
