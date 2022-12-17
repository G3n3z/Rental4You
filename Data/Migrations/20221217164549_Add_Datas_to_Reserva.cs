using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental4You.Data.Migrations
{
    public partial class Add_Datas_to_Reserva : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataEntrega",
                table: "Reservas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DataLevantamento",
                table: "Reservas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataEntrega",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "DataLevantamento",
                table: "Reservas");
        }
    }
}
