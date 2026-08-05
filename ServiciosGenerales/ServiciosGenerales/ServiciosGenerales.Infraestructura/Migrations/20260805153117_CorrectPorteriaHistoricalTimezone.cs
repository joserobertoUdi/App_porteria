using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiciosGenerales.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class CorrectPorteriaHistoricalTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El registro 11 fue creado con la salida una hora antes de la entrada.
            // Se limita a la huella exacta detectada para no alterar datos distintos.
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM dbo.RegistrosPorteria
                    WHERE Id = 11
                      AND FechaEntrada = CONVERT(datetime2(7), '2026-07-31T15:43:37.1750199', 126)
                      AND FechaSalida = CONVERT(datetime2(7), '2026-07-31T14:43:50.8660750', 126)
                )
                BEGIN
                    UPDATE dbo.RegistrosPorteria
                    SET FechaSalida = DATEADD(HOUR, 1, FechaSalida)
                    WHERE Id = 11
                      AND FechaEntrada = CONVERT(datetime2(7), '2026-07-31T15:43:37.1750199', 126)
                      AND FechaSalida = CONVERT(datetime2(7), '2026-07-31T14:43:50.8660750', 126);
                END;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.RegistrosPorteria
                    WHERE FechaSalida IS NOT NULL
                      AND FechaSalida < FechaEntrada
                )
                    THROW 51001, 'Existen registros de porteria con salida anterior a la entrada.', 1;

                ALTER TABLE dbo.RegistrosPorteria
                WITH CHECK CHECK CONSTRAINT CK_RegistrosPorteria_Fechas;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se revierte una correccion historica, para no reintroducir una fecha invalida.
        }
    }
}
