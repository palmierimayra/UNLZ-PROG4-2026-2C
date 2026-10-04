using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPLudoteca.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialLudoteca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriaJuegos",
                columns: table => new
                {
                    IdCategoriaJuego = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescripcionCategoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Audit_IdUsuarioAlta = table.Column<int>(type: "int", nullable: false),
                    Audit_FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_IdUsuarioModificacion = table.Column<int>(type: "int", nullable: true),
                    Audit_FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Audit_IdUsuarioBaja = table.Column<int>(type: "int", nullable: true),
                    Audit_FechaBaja = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaJuegos", x => x.IdCategoriaJuego);
                });

            migrationBuilder.CreateTable(
                name: "Juegos",
                columns: table => new
                {
                    IdJuego = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescripcionJuego = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdCategoriaJuego = table.Column<int>(type: "int", nullable: false),
                    Audit_IdUsuarioAlta = table.Column<int>(type: "int", nullable: false),
                    Audit_FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_IdUsuarioModificacion = table.Column<int>(type: "int", nullable: true),
                    Audit_FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Audit_IdUsuarioBaja = table.Column<int>(type: "int", nullable: true),
                    Audit_FechaBaja = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Juegos", x => x.IdJuego);
                    table.ForeignKey(
                        name: "FK_Juegos_CategoriaJuegos_IdCategoriaJuego",
                        column: x => x.IdCategoriaJuego,
                        principalTable: "CategoriaJuegos",
                        principalColumn: "IdCategoriaJuego",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alquileres",
                columns: table => new
                {
                    IdAlquiler = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdJuego = table.Column<int>(type: "int", nullable: false),
                    FechaRetiro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaDevolucionEstimada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaDevolucionReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoAlquiler = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiasDemora = table.Column<int>(type: "int", nullable: true),
                    Recargo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MontoRecargo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Audit_IdUsuarioAlta = table.Column<int>(type: "int", nullable: false),
                    Audit_FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_IdUsuarioModificacion = table.Column<int>(type: "int", nullable: true),
                    Audit_FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Audit_IdUsuarioBaja = table.Column<int>(type: "int", nullable: true),
                    Audit_FechaBaja = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alquileres", x => x.IdAlquiler);
                    table.ForeignKey(
                        name: "FK_Alquileres_AspNetUsers_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alquileres_Juegos_IdJuego",
                        column: x => x.IdJuego,
                        principalTable: "Juegos",
                        principalColumn: "IdJuego",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alquileres_IdJuego",
                table: "Alquileres",
                column: "IdJuego");

            migrationBuilder.CreateIndex(
                name: "IX_Alquileres_IdUsuario",
                table: "Alquileres",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Juegos_IdCategoriaJuego",
                table: "Juegos",
                column: "IdCategoriaJuego");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alquileres");

            migrationBuilder.DropTable(
                name: "Juegos");

            migrationBuilder.DropTable(
                name: "CategoriaJuegos");
        }
    }
}
