using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiciosGenerales.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class EnforceIntegrityAndRestrictDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Usuarios_UsuarioId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosParqueo_Vehiculos_VehiculoId",
                table: "RegistrosParqueo");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosPorteria_Usuarios_UsuarioId",
                table: "RegistrosPorteria");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_TiposUsuario_TipoUsuarioId",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_Usuarios_UsuarioId",
                table: "Vehiculos");

            migrationBuilder.CreateIndex(
                name: "IX_TiposUsuario_Nombre",
                table: "TiposUsuario",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            // WITH NOCHECK conserva datos históricos que ya estaban inválidos,
            // pero valida todas las inserciones y actualizaciones posteriores.
            migrationBuilder.Sql(
                "ALTER TABLE [RegistrosPorteria] WITH NOCHECK ADD CONSTRAINT [CK_RegistrosPorteria_Fechas] CHECK ([FechaSalida] IS NULL OR [FechaSalida] >= [FechaEntrada]);");

            migrationBuilder.Sql(
                "ALTER TABLE [RegistrosParqueo] WITH NOCHECK ADD CONSTRAINT [CK_RegistrosParqueo_Fechas] CHECK ([FechaSalida] IS NULL OR [FechaSalida] >= [FechaIngreso]);");

            migrationBuilder.Sql(
                "ALTER TABLE [RefreshTokens] WITH NOCHECK ADD CONSTRAINT [CK_RefreshTokens_Fechas] CHECK ([FechaExpiracion] > [FechaCreacion] AND ([FechaRevocacion] IS NULL OR [FechaRevocacion] >= [FechaCreacion]));");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Usuarios_UsuarioId",
                table: "RefreshTokens",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosParqueo_Vehiculos_VehiculoId",
                table: "RegistrosParqueo",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosPorteria_Usuarios_UsuarioId",
                table: "RegistrosPorteria",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_TiposUsuario_TipoUsuarioId",
                table: "Usuarios",
                column: "TipoUsuarioId",
                principalTable: "TiposUsuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_Usuarios_UsuarioId",
                table: "Vehiculos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Usuarios_UsuarioId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosParqueo_Vehiculos_VehiculoId",
                table: "RegistrosParqueo");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosPorteria_Usuarios_UsuarioId",
                table: "RegistrosPorteria");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_TiposUsuario_TipoUsuarioId",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_Usuarios_UsuarioId",
                table: "Vehiculos");

            migrationBuilder.DropIndex(
                name: "IX_TiposUsuario_Nombre",
                table: "TiposUsuario");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Nombre",
                table: "Roles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrosPorteria_Fechas",
                table: "RegistrosPorteria");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrosParqueo_Fechas",
                table: "RegistrosParqueo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RefreshTokens_Fechas",
                table: "RefreshTokens");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Usuarios_UsuarioId",
                table: "RefreshTokens",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosParqueo_Vehiculos_VehiculoId",
                table: "RegistrosParqueo",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosPorteria_Usuarios_UsuarioId",
                table: "RegistrosPorteria",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_TiposUsuario_TipoUsuarioId",
                table: "Usuarios",
                column: "TipoUsuarioId",
                principalTable: "TiposUsuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_Usuarios_UsuarioId",
                table: "Vehiculos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
