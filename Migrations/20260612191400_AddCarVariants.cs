using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCarVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Engines are re-parented from models to variants; existing rows reference
            // models that are not variants, so clear them before swapping the foreign key.
            migrationBuilder.Sql("DELETE FROM \"CarEngines\";");

            migrationBuilder.DropForeignKey(
                name: "FK_CarEngines_CarModels_ModelId",
                table: "CarEngines");

            migrationBuilder.RenameColumn(
                name: "ModelId",
                table: "CarEngines",
                newName: "VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_CarEngines_ModelId_Name",
                table: "CarEngines",
                newName: "IX_CarEngines_VariantId_Name");

            migrationBuilder.AddColumn<string>(
                name: "Trim",
                table: "DynoRuns",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CarVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModelId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarVariants_CarModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "CarModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarVariants_ModelId_Name",
                table: "CarVariants",
                columns: new[] { "ModelId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CarEngines_CarVariants_VariantId",
                table: "CarEngines",
                column: "VariantId",
                principalTable: "CarVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarEngines_CarVariants_VariantId",
                table: "CarEngines");

            migrationBuilder.DropTable(
                name: "CarVariants");

            migrationBuilder.DropColumn(
                name: "Trim",
                table: "DynoRuns");

            migrationBuilder.RenameColumn(
                name: "VariantId",
                table: "CarEngines",
                newName: "ModelId");

            migrationBuilder.RenameIndex(
                name: "IX_CarEngines_VariantId_Name",
                table: "CarEngines",
                newName: "IX_CarEngines_ModelId_Name");

            migrationBuilder.AddForeignKey(
                name: "FK_CarEngines_CarModels_ModelId",
                table: "CarEngines",
                column: "ModelId",
                principalTable: "CarModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
