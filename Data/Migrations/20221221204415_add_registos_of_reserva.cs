using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental4You.Data.Migrations
{
    public partial class add_registos_of_reserva : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntregaId",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LevantamentoId",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservaId",
                table: "Registos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_EntregaId",
                table: "Reservas",
                column: "EntregaId",
                unique: true,
                filter: "[EntregaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_LevantamentoId",
                table: "Reservas",
                column: "LevantamentoId",
                unique: true,
                filter: "[LevantamentoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Registos_ReservaId",
                table: "Registos",
                column: "ReservaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registos_Reservas_ReservaId",
                table: "Registos",
                column: "ReservaId",
                principalTable: "Reservas",
                principalColumn: "ReservaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Registos_EntregaId",
                table: "Reservas",
                column: "EntregaId",
                principalTable: "Registos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Registos_LevantamentoId",
                table: "Reservas",
                column: "LevantamentoId",
                principalTable: "Registos",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registos_Reservas_ReservaId",
                table: "Registos");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Registos_EntregaId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Registos_LevantamentoId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_EntregaId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_LevantamentoId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Registos_ReservaId",
                table: "Registos");

            migrationBuilder.DropColumn(
                name: "EntregaId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "LevantamentoId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "ReservaId",
                table: "Registos");
        }
    }
}
