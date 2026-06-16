using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nstuning_api.Migrations
{
    /// <inheritdoc />
    public partial class AddDynoHubEngineFigures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TorqueBeforeNm",
                table: "DynoRuns",
                newName: "HubTorqueBeforeWnm");

            migrationBuilder.RenameColumn(
                name: "TorqueAfterNm",
                table: "DynoRuns",
                newName: "HubTorqueAfterWnm");

            migrationBuilder.RenameColumn(
                name: "PowerBeforeHp",
                table: "DynoRuns",
                newName: "HubPowerBeforeWhp");

            migrationBuilder.RenameColumn(
                name: "PowerAfterHp",
                table: "DynoRuns",
                newName: "HubPowerAfterWhp");

            migrationBuilder.AddColumn<int>(
                name: "EnginePowerAfterHp",
                table: "DynoRuns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnginePowerBeforeHp",
                table: "DynoRuns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EngineTorqueAfterNm",
                table: "DynoRuns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EngineTorqueBeforeNm",
                table: "DynoRuns",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnginePowerAfterHp",
                table: "DynoRuns");

            migrationBuilder.DropColumn(
                name: "EnginePowerBeforeHp",
                table: "DynoRuns");

            migrationBuilder.DropColumn(
                name: "EngineTorqueAfterNm",
                table: "DynoRuns");

            migrationBuilder.DropColumn(
                name: "EngineTorqueBeforeNm",
                table: "DynoRuns");

            migrationBuilder.RenameColumn(
                name: "HubTorqueBeforeWnm",
                table: "DynoRuns",
                newName: "TorqueBeforeNm");

            migrationBuilder.RenameColumn(
                name: "HubTorqueAfterWnm",
                table: "DynoRuns",
                newName: "TorqueAfterNm");

            migrationBuilder.RenameColumn(
                name: "HubPowerBeforeWhp",
                table: "DynoRuns",
                newName: "PowerBeforeHp");

            migrationBuilder.RenameColumn(
                name: "HubPowerAfterWhp",
                table: "DynoRuns",
                newName: "PowerAfterHp");
        }
    }
}
