using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental4You.Data.Migrations
{
    public partial class remove_registos_of_reserva : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "EntregaId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "LevantamentoId",
                table: "Reservas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_EntregaId",
                table: "Reservas",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_LevantamentoId",
                table: "Reservas",
                column: "LevantamentoId");

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
    }
}
