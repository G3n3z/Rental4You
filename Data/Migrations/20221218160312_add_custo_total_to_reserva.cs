using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental4You.Data.Migrations
{
    public partial class add_custo_total_to_reserva : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CustoTotal",
                table: "Reservas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustoTotal",
                table: "Reservas");
        }
    }
}
