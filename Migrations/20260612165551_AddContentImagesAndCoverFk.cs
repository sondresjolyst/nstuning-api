using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class AddContentImagesAndCoverFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageContentType",
                table: "DynoRuns");

            migrationBuilder.DropColumn(
                name: "CoverImageData",
                table: "DynoRuns");

            migrationBuilder.AddColumn<int>(
                name: "CoverImageId",
                table: "DynoRuns",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContentImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoredPath = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    UploadedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentImages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynoRuns_CoverImageId",
                table: "DynoRuns",
                column: "CoverImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_DynoRuns_ContentImages_CoverImageId",
                table: "DynoRuns",
                column: "CoverImageId",
                principalTable: "ContentImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DynoRuns_ContentImages_CoverImageId",
                table: "DynoRuns");

            migrationBuilder.DropTable(
                name: "ContentImages");

            migrationBuilder.DropIndex(
                name: "IX_DynoRuns_CoverImageId",
                table: "DynoRuns");

            migrationBuilder.DropColumn(
                name: "CoverImageId",
                table: "DynoRuns");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageContentType",
                table: "DynoRuns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageData",
                table: "DynoRuns",
                type: "text",
                nullable: true);
        }
    }
}
