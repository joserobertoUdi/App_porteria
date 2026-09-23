using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiciosGenerales.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistradoPorAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegistradoPorUsuarioId",
                table: "RegistrosPorteria",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegistradoPorUsuarioId",
                table: "RegistrosParqueo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosPorteria_RegistradoPorUsuarioId",
                table: "RegistrosPorteria",
                column: "RegistradoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosParqueo_RegistradoPorUsuarioId",
                table: "RegistrosParqueo",
                column: "RegistradoPorUsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosParqueo_Usuarios_RegistradoPorUsuarioId",
                table: "RegistrosParqueo",
                column: "RegistradoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosPorteria_Usuarios_RegistradoPorUsuarioId",
                table: "RegistrosPorteria",
                column: "RegistradoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosParqueo_Usuarios_RegistradoPorUsuarioId",
                table: "RegistrosParqueo");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosPorteria_Usuarios_RegistradoPorUsuarioId",
                table: "RegistrosPorteria");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosPorteria_RegistradoPorUsuarioId",
                table: "RegistrosPorteria");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosParqueo_RegistradoPorUsuarioId",
                table: "RegistrosParqueo");

            migrationBuilder.DropColumn(
                name: "RegistradoPorUsuarioId",
                table: "RegistrosPorteria");

            migrationBuilder.DropColumn(
                name: "RegistradoPorUsuarioId",
                table: "RegistrosParqueo");
        }
    }
}
