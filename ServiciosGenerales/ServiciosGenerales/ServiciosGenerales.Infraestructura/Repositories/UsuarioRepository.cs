using Dapper;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    /// <summary>
    /// Repositorio de usuarios implementado con Dapper.
    /// Consultas contra la tabla Usuarios con JOIN a Roles y TiposUsuario.
    /// </summary>
    public sealed class UsuarioRepository : IUsuarioRepository
    {
        private readonly DapperContext _dapper;

        public UsuarioRepository(DapperContext dapper)
        {
            _dapper = dapper;
        }

        /// <summary>
        /// Busca un usuario por documento de identidad, incluyendo su rol.
        /// Equivalente a: <c>Include(u => u.Rol).FirstOrDefaultAsync(...)</c>.
        /// </summary>
        public async Task<Usuario?> ObtenerPorDocumentoAsync(string documento)
        {
            const string sql = """
                SELECT
                    u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                    u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                    u.IntentosFallidos, u.BloqueoHasta,
                    r.Id, r.Nombre
                FROM Usuarios u
                LEFT JOIN Roles r ON u.RolId = r.Id
                WHERE u.DocumentoIdentidad = @Documento
                """;

            using var conn = _dapper.CrearConexion();

            var resultado = await conn.QueryAsync<Usuario, Rol?, Usuario>(
                sql,
                (usuario, rol) =>
                {
                    usuario.Rol = rol;
                    return usuario;
                },
                new { Documento = documento },
                splitOn: "Id,Id");

            return resultado.FirstOrDefault();
        }

        /// <summary>
        /// Busca un usuario por Id, incluyendo su rol.
        /// </summary>
        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            const string sql = """
                SELECT
                    u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                    u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                    u.IntentosFallidos, u.BloqueoHasta,
                    r.Id, r.Nombre
                FROM Usuarios u
                LEFT JOIN Roles r ON u.RolId = r.Id
                WHERE u.Id = @Id
                """;

            using var conn = _dapper.CrearConexion();

            var resultado = await conn.QueryAsync<Usuario, Rol?, Usuario>(
                sql,
                (usuario, rol) =>
                {
                    usuario.Rol = rol;
                    return usuario;
                },
                new { Id = id },
                splitOn: "Id,Id");

            return resultado.FirstOrDefault();
        }

        /// <summary>
        /// Busca usuarios cuyo documento o nombre comience con / contenga el
        /// término de búsqueda. Solo usuarios activos, top 5.
        /// Equivalente a: <c>Where(StartsWith || Contains).Take(5)</c>.
        /// </summary>
        public async Task<IEnumerable<Usuario>> ObtenerCoincidenciaAsync(string termino)
        {
            const string sql = """
                SELECT TOP (5)
                    u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                    u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                    u.IntentosFallidos, u.BloqueoHasta
                FROM Usuarios u
                WHERE u.Estado = 1
                  AND (u.DocumentoIdentidad LIKE @Termino + '%'
                       OR u.NombreCompleto LIKE '%' + @Termino + '%')
                ORDER BY u.NombreCompleto
                """;

            using var conn = _dapper.CrearConexion();
            return await conn.QueryAsync<Usuario>(sql, new { Termino = termino });
        }

        /// <summary>
        /// Obtiene todos los usuarios con sus relaciones (Rol, TipoUsuario).
        /// Equivalente a: <c>Include(u => u.Rol).Include(u => u.TipoUsuario)</c>.
        /// </summary>
        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            const string sql = """
                SELECT
                    u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                    u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                    u.IntentosFallidos, u.BloqueoHasta,
                    r.Id, r.Nombre,
                    t.Id, t.Nombre
                FROM Usuarios u
                LEFT JOIN Roles r ON u.RolId = r.Id
                LEFT JOIN TiposUsuario t ON u.TipoUsuarioId = t.Id
                ORDER BY u.FechaRegistro DESC
                """;

            using var conn = _dapper.CrearConexion();

            var resultado = await conn.QueryAsync<Usuario, Rol?, TiposUsuario?, Usuario>(
                sql,
                (usuario, rol, tipo) =>
                {
                    usuario.Rol = rol;
                    usuario.TipoUsuario = tipo;
                    return usuario;
                },
                splitOn: "Id,Id,Id");

            return resultado;
        }

        /// <summary>
        /// Crea un usuario y devuelve su Id generado.
        /// </summary>
        public async Task<int> CrearAsync(Usuario usuario)
        {
            const string sql = """
                INSERT INTO Usuarios
                    (NombreCompleto, DocumentoIdentidad, TipoUsuarioId, FotoUrl,
                     PasswordHash, RolId, FechaRegistro, Estado, IntentosFallidos, BloqueoHasta)
                VALUES
                    (@NombreCompleto, @DocumentoIdentidad, @TipoUsuarioId, @FotoUrl,
                     @PasswordHash, @RolId, @FechaRegistro, @Estado, @IntentosFallidos, @BloqueoHasta);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            using var conn = _dapper.CrearConexion();
            await conn.OpenAsync();

            var id = await conn.ExecuteScalarAsync<int>(sql, new
            {
                usuario.NombreCompleto,
                usuario.DocumentoIdentidad,
                usuario.TipoUsuarioId,
                usuario.FotoUrl,
                usuario.PasswordHash,
                usuario.RolId,
                usuario.FechaRegistro,
                usuario.Estado,
                usuario.IntentosFallidos,
                usuario.BloqueoHasta
            });

            usuario.Id = id;
            return id;
        }

        /// <summary>
        /// Actualiza todos los campos de un usuario existente.
        /// </summary>
        public async Task<bool> ActualizarAsync(Usuario usuario)
        {
            const string sql = """
                UPDATE Usuarios
                SET NombreCompleto = @NombreCompleto,
                    DocumentoIdentidad = @DocumentoIdentidad,
                    TipoUsuarioId = @TipoUsuarioId,
                    FotoUrl = @FotoUrl,
                    PasswordHash = @PasswordHash,
                    RolId = @RolId,
                    Estado = @Estado,
                    IntentosFallidos = @IntentosFallidos,
                    BloqueoHasta = @BloqueoHasta
                WHERE Id = @Id
                """;

            using var conn = _dapper.CrearConexion();
            var filas = await conn.ExecuteAsync(sql, new
            {
                usuario.NombreCompleto,
                usuario.DocumentoIdentidad,
                usuario.TipoUsuarioId,
                usuario.FotoUrl,
                usuario.PasswordHash,
                usuario.RolId,
                usuario.Estado,
                usuario.IntentosFallidos,
                usuario.BloqueoHasta,
                usuario.Id
            });

            return filas > 0;
        }
    }
}
