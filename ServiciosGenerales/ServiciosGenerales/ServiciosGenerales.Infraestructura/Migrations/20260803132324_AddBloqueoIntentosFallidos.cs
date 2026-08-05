using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiciosGenerales.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddBloqueoIntentosFallidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueoHasta",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloqueoHasta",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                table: "Usuarios");
        }
    }
}
