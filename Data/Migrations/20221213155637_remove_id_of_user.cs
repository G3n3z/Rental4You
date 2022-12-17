using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental4You.Data.Migrations
{
    public partial class remove_id_of_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registos_AspNetUsers_FuncionarioId",
                table: "Registos");

            migrationBuilder.AlterColumn<string>(
                name: "FuncionarioId",
                table: "Registos",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Registos_AspNetUsers_FuncionarioId",
                table: "Registos",
                column: "FuncionarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registos_AspNetUsers_FuncionarioId",
                table: "Registos");

            migrationBuilder.AlterColumn<string>(
                name: "FuncionarioId",
                table: "Registos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Registos_AspNetUsers_FuncionarioId",
                table: "Registos",
                column: "FuncionarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
