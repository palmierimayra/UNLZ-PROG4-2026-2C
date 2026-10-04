using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPLudoteca.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCantidadDisponibleJuego : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "Juegos",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Disponible",
                table: "Juegos",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "Juegos");

            migrationBuilder.DropColumn(
                name: "Disponible",
                table: "Juegos");
        }
    }
}
