using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiciosGenerales.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitanteRoleYIndicesActivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre" },
                values: new object[] { 4, "Visitante" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosPorteria_FechaSalida",
                table: "RegistrosPorteria",
                column: "FechaSalida",
                filter: "[FechaSalida] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosParqueo_FechaSalida",
                table: "RegistrosParqueo",
                column: "FechaSalida",
                filter: "[FechaSalida] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistrosPorteria_FechaSalida",
                table: "RegistrosPorteria");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosParqueo_FechaSalida",
                table: "RegistrosParqueo");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
