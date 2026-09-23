using Dapper;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    /// <summary>
    /// Repositorio de registros de parqueo implementado con Dapper.
    /// Consultas contra la tabla RegistrosParqueo con JOINs a Vehiculos,
    /// Usuarios, TiposUsuario y Roles.
    /// </summary>
    public sealed class RegistroParqueoRepository : IRegistroParqueoRepository
    {
        private readonly DapperContext _dapper;

        public RegistroParqueoRepository(DapperContext dapper)
        {
            _dapper = dapper;
        }

        // ── SQL base para SELECT con relaciones completas ──
        //
        // Maps el patrón de EF Core:
        //   Include(r => r.Vehiculo)
        //     .ThenInclude(v => v.Usuario)
        //       .ThenInclude(u => u.TipoUsuario)
        //
        // Además trae el operador que aprobó el ingreso (RegistradoPor) con
        // LEFT JOIN por si el registro es anterior a la auditoría de guardia.

        /// <summary>
        /// Select base con visitante (v→u) y operador (rp.RegistradoPorUsuarioId → op).
        /// </summary>
        private const string SelectSql = """
            SELECT
                rp.Id, rp.VehiculoId, rp.RegistradoPorUsuarioId, rp.FechaIngreso, rp.FechaSalida, rp.PuertaAcceso, rp.Observaciones,
                v.Id, v.UsuarioId, v.Matricula, v.Marca, v.Modelo, v.Color, v.Activo, v.FechaRegistro,
                u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                u.IntentosFallidos, u.BloqueoHasta,
                op.Id, op.NombreCompleto, op.DocumentoIdentidad, op.TipoUsuarioId,
                op.FotoUrl, op.PasswordHash, op.RolId, op.FechaRegistro, op.Estado,
                op.IntentosFallidos, op.BloqueoHasta
            FROM RegistrosParqueo rp
            INNER JOIN Vehiculos v ON rp.VehiculoId = v.Id
            INNER JOIN Usuarios u ON v.UsuarioId = u.Id
            LEFT JOIN Usuarios op ON rp.RegistradoPorUsuarioId = op.Id
            """;

        /// <summary>
        /// Inserta un registro de entrada de parqueo y devuelve su Id.
        /// </summary>
        public async Task<int> RegistrarEntradaAsync(RegistroParqueo registro)
        {
            const string sql = """
                INSERT INTO RegistrosParqueo
                    (VehiculoId, RegistradoPorUsuarioId, FechaIngreso, PuertaAcceso, Observaciones)
                VALUES
                    (@VehiculoId, @RegistradoPorUsuarioId, @FechaIngreso, @PuertaAcceso, @Observaciones);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            using var conn = _dapper.CrearConexion();
            await conn.OpenAsync();

            var id = await conn.ExecuteScalarAsync<int>(sql, new
            {
                registro.VehiculoId,
                registro.RegistradoPorUsuarioId,
                registro.FechaIngreso,
                registro.PuertaAcceso,
                registro.Observaciones
            });

            registro.Id = id;
            return id;
        }

        /// <summary>
        /// Registra la salida de un vehículo. Devuelve false si el registro no existe.
        /// </summary>
        public async Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida)
        {
            const string sql = """
                UPDATE RegistrosParqueo
                SET FechaSalida = @FechaSalida
                WHERE Id = @Id
                """;

            using var conn = _dapper.CrearConexion();
            var filas = await conn.ExecuteAsync(sql, new { Id = id, FechaSalida = fechaSalida });
            return filas > 0;
        }

        /// <summary>
        /// Obtiene los registros activos (sin salida) con relaciones completas.
        /// </summary>
        public async Task<IEnumerable<RegistroParqueo>> ObtenerActivosAsync()
        {
            const string sql = SelectSql + " WHERE rp.FechaSalida IS NULL ORDER BY rp.FechaIngreso DESC";

            using var conn = _dapper.CrearConexion();

            return await conn.QueryAsync<RegistroParqueo, Vehiculo, Usuario, Usuario, RegistroParqueo>(
                sql,
                (registro, vehiculo, usuario, operador) =>
                {
                    vehiculo.Usuario = usuario;
                    registro.Vehiculo = vehiculo;
                    registro.RegistradoPor = operador;
                    return registro;
                },
                splitOn: "Id,Id,Id,Id");
        }

        /// <summary>
        /// Obtiene los registros de un vehículo específico.
        /// </summary>
        public async Task<IEnumerable<RegistroParqueo>> ObtenerPorVehiculoIdAsync(int vehiculoId)
        {
            const string sql = """
                SELECT
                    rp.Id, rp.VehiculoId, rp.FechaIngreso, rp.FechaSalida, rp.PuertaAcceso, rp.Observaciones,
                    v.Id, v.UsuarioId, v.Matricula, v.Marca, v.Modelo, v.Color, v.Activo, v.FechaRegistro
                FROM RegistrosParqueo rp
                INNER JOIN Vehiculos v ON rp.VehiculoId = v.Id
                WHERE rp.VehiculoId = @VehiculoId
                ORDER BY rp.FechaIngreso DESC
                """;

            using var conn = _dapper.CrearConexion();

            var resultado = await conn.QueryAsync<RegistroParqueo, Vehiculo, RegistroParqueo>(
                sql,
                (registro, vehiculo) =>
                {
                    registro.Vehiculo = vehiculo;
                    return registro;
                },
                new { VehiculoId = vehiculoId },
                splitOn: "Id,Id");

            return resultado;
        }

        /// <summary>
        /// Obtiene los registros de todos los vehículos de un usuario (visitante).
        /// </summary>
        public async Task<IEnumerable<RegistroParqueo>> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            const string sql = SelectSql + " WHERE v.UsuarioId = @UsuarioId ORDER BY rp.FechaIngreso DESC";

            using var conn = _dapper.CrearConexion();

            return await conn.QueryAsync<RegistroParqueo, Vehiculo, Usuario, Usuario, RegistroParqueo>(
                sql,
                (registro, vehiculo, usuario, operador) =>
                {
                    vehiculo.Usuario = usuario;
                    registro.Vehiculo = vehiculo;
                    registro.RegistradoPor = operador;
                    return registro;
                },
                new { UsuarioId = usuarioId },
                splitOn: "Id,Id,Id,Id");
        }

        /// <summary>
        /// Obtiene los registros de parqueo aprobados por un operador específico
        /// (guardia/portero). Es la base de la auditoría por día y turno.
        /// </summary>
        public async Task<IEnumerable<RegistroParqueo>> ObtenerPorRegistradoPorIdAsync(int registradoPorUsuarioId)
        {
            const string sql = SelectSql + " WHERE rp.RegistradoPorUsuarioId = @RegistradoPorUsuarioId ORDER BY rp.FechaIngreso DESC";

            using var conn = _dapper.CrearConexion();

            return await conn.QueryAsync<RegistroParqueo, Vehiculo, Usuario, Usuario, RegistroParqueo>(
                sql,
                (registro, vehiculo, usuario, operador) =>
                {
                    vehiculo.Usuario = usuario;
                    registro.Vehiculo = vehiculo;
                    registro.RegistradoPor = operador;
                    return registro;
                },
                new { RegistradoPorUsuarioId = registradoPorUsuarioId },
                splitOn: "Id,Id,Id,Id");
        }

        /// <summary>
        /// Obtiene todo el historial de parqueo con relaciones completas.
        /// Equivalente a:
        ///   <c>Include(r => r.Vehiculo).ThenInclude(v => v.Usuario).ThenInclude(u => u.TipoUsuario)</c>.
        /// </summary>
        public async Task<IEnumerable<RegistroParqueo>> ObtenerHistorialAsync()
        {
            const string sql = SelectSql + " ORDER BY rp.FechaIngreso DESC";

            using var conn = _dapper.CrearConexion();

            return await conn.QueryAsync<RegistroParqueo, Vehiculo, Usuario, Usuario, RegistroParqueo>(
                sql,
                (registro, vehiculo, usuario, operador) =>
                {
                    vehiculo.Usuario = usuario;
                    registro.Vehiculo = vehiculo;
                    registro.RegistradoPor = operador;
                    return registro;
                },
                splitOn: "Id,Id,Id,Id");
        }
    }
}
