using Dapper;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    /// <summary>
    /// Repositorio de registros de portería implementado con Dapper.
    /// Consultas contra la tabla RegistrosPorteria con JOIN a Usuarios y TiposUsuario.
    /// </summary>
    public sealed class RegistroPorteriaRepository : IRegistroPorteriaRepository
    {
        private readonly DapperContext _dapper;

        public RegistroPorteriaRepository(DapperContext dapper)
        {
            _dapper = dapper;
        }

        /// <summary>
        /// Columna base para consultas que traen el visitante (Usuario) y el
        /// operador que aprobó el ingreso (RegistradoPor), con LEFT JOIN por si
        /// el registro es anterior a la funcionalidad de auditoría.
        /// </summary>
        private const string SelectSql = """
            SELECT
                rp.Id, rp.UsuarioId, rp.RegistradoPorUsuarioId, rp.FechaEntrada, rp.FechaSalida,
                rp.PuertaEntrada, rp.PuertaSalida, rp.MotivoVisita, rp.AreaDestino, rp.FotoUrl,
                u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                u.IntentosFallidos, u.BloqueoHasta,
                op.Id, op.NombreCompleto, op.DocumentoIdentidad, op.TipoUsuarioId,
                op.FotoUrl, op.PasswordHash, op.RolId, op.FechaRegistro, op.Estado,
                op.IntentosFallidos, op.BloqueoHasta
            FROM RegistrosPorteria rp
            INNER JOIN Usuarios u ON rp.UsuarioId = u.Id
            LEFT JOIN Usuarios op ON rp.RegistradoPorUsuarioId = op.Id
            """;

        /// <summary>Inserta un registro de entrada y devuelve el Id generado.</summary>
        public async Task<int> RegistrarEntradaAsync(RegistroPorteria registro)
        {
            const string sql = """
                INSERT INTO RegistrosPorteria
                    (UsuarioId, RegistradoPorUsuarioId, FechaEntrada, PuertaEntrada,
                     MotivoVisita, AreaDestino, FotoUrl)
                VALUES
                    (@UsuarioId, @RegistradoPorUsuarioId, @FechaEntrada, @PuertaEntrada,
                     @MotivoVisita, @AreaDestino, @FotoUrl);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            using var conn = _dapper.CrearConexion();
            await conn.OpenAsync();

            var id = await conn.ExecuteScalarAsync<int>(sql, new
            {
                registro.UsuarioId,
                registro.RegistradoPorUsuarioId,
                registro.FechaEntrada,
                registro.PuertaEntrada,
                registro.MotivoVisita,
                registro.AreaDestino,
                registro.FotoUrl
            });

            registro.Id = id;
            return id;
        }

        /// <summary>
        /// Actualiza la fecha de salida y puerta de salida de un registro.
        /// Devuelve false si el registro no existe.
        /// </summary>
        public async Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida, string? puertaSalida)
        {
            const string sql = """
                UPDATE RegistrosPorteria
                SET FechaSalida = @FechaSalida, PuertaSalida = @PuertaSalida
                WHERE Id = @Id
                """;

            using var conn = _dapper.CrearConexion();
            var filas = await conn.ExecuteAsync(sql, new { Id = id, FechaSalida = fechaSalida, PuertaSalida = puertaSalida });
            return filas > 0;
        }

        /// <summary>
        /// Obtiene todas las visitas activas (sin salida) con su visitante y operador.
        /// Equivalente a: <c>Include(r => r.Usuario).Include(r => r.RegistradoPor).Where(r => r.FechaSalida == null)</c>.
        /// </summary>
        public async Task<IEnumerable<RegistroPorteria>> ObtenerVisitasActivasAsync()
        {
            const string sql = SelectSql + " WHERE rp.FechaSalida IS NULL ORDER BY rp.FechaEntrada DESC";

            using var conn = _dapper.CrearConexion();
            return await conn.QueryAsync<RegistroPorteria, Usuario, Usuario, RegistroPorteria>(
                sql,
                (registro, usuario, operador) =>
                {
                    registro.Usuario = usuario;
                    registro.RegistradoPor = operador;
                    return registro;
                },
                splitOn: "Id,Id,Id");
        }

        /// <summary>
        /// Obtiene todo el historial de registros de portería con visitante y operador.
        /// Equivalente a: <c>Include(r => r.Usuario).Include(r => r.RegistradoPor)</c>.
        /// </summary>
        public async Task<IEnumerable<RegistroPorteria>> ObtenerHistorialAsync()
        {
            const string sql = SelectSql + " ORDER BY rp.FechaEntrada DESC";

            using var conn = _dapper.CrearConexion();
            return await conn.QueryAsync<RegistroPorteria, Usuario, Usuario, RegistroPorteria>(
                sql,
                (registro, usuario, operador) =>
                {
                    registro.Usuario = usuario;
                    registro.RegistradoPor = operador;
                    return registro;
                },
                splitOn: "Id,Id,Id");
        }

        /// <summary>
        /// Obtiene los registros de portería de un usuario específico (visitante).
        /// </summary>
        public async Task<IEnumerable<RegistroPorteria>> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            const string sql = SelectSql + " WHERE rp.UsuarioId = @UsuarioId ORDER BY rp.FechaEntrada DESC";

            using var conn = _dapper.CrearConexion();
            return await conn.QueryAsync<RegistroPorteria, Usuario, Usuario, RegistroPorteria>(
                sql,
                (registro, usuario, operador) =>
                {
                    registro.Usuario = usuario;
                    registro.RegistradoPor = operador;
                    return registro;
                },
                new { UsuarioId = usuarioId },
                splitOn: "Id,Id,Id");
        }

        /// <summary>
        /// Obtiene los registros de portería aprobados por un operador específico
        /// (guardia/portero). Es la base de la auditoría por día y turno.
        /// </summary>
        public async Task<IEnumerable<RegistroPorteria>> ObtenerPorRegistradoPorIdAsync(int registradoPorUsuarioId)
        {
            const string sql = SelectSql + " WHERE rp.RegistradoPorUsuarioId = @RegistradoPorUsuarioId ORDER BY rp.FechaEntrada DESC";

            using var conn = _dapper.CrearConexion();
            return await conn.QueryAsync<RegistroPorteria, Usuario, Usuario, RegistroPorteria>(
                sql,
                (registro, usuario, operador) =>
                {
                    registro.Usuario = usuario;
                    registro.RegistradoPor = operador;
                    return registro;
                },
                new { RegistradoPorUsuarioId = registradoPorUsuarioId },
                splitOn: "Id,Id,Id");
        }
    }
}
