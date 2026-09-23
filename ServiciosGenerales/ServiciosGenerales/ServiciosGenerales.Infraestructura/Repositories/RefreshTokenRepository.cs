using Dapper;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    /// <summary>
    /// Repositorio de refresh tokens implementado con Dapper.
    /// Consultas contra la tabla RefreshTokens con JOIN a Usuarios y Roles.
    /// </summary>
    public sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DapperContext _dapper;

        public RefreshTokenRepository(DapperContext dapper)
        {
            _dapper = dapper;
        }

        /// <summary>
        /// Busca un refresh token por su hash, incluyendo el usuario y rol asociados.
        /// Equivalente a EF Core: <c>Include(r => r.Usuario).ThenInclude(u => u.Rol)</c>.
        /// </summary>
        public async Task<RefreshToken?> ObtenerPorHashAsync(string tokenHash)
        {
            const string sql = """
                SELECT
                    rt.Id, rt.UsuarioId, rt.TokenHash, rt.FechaExpiracion,
                    rt.FechaCreacion, rt.FechaRevocacion, rt.ReemplazadoPor,
                    u.Id, u.NombreCompleto, u.DocumentoIdentidad, u.TipoUsuarioId,
                    u.FotoUrl, u.PasswordHash, u.RolId, u.FechaRegistro, u.Estado,
                    u.IntentosFallidos, u.BloqueoHasta,
                    r.Id, r.Nombre
                FROM RefreshTokens rt
                INNER JOIN Usuarios u ON rt.UsuarioId = u.Id
                LEFT JOIN Roles r ON u.RolId = r.Id
                WHERE rt.TokenHash = @TokenHash
                """;

            using var conn = _dapper.CrearConexion();

            var resultado = await conn.QueryAsync<RefreshToken, Usuario, Rol?, RefreshToken>(
                sql,
                (token, usuario, rol) =>
                {
                    usuario.Rol = rol;
                    token.Usuario = usuario;
                    return token;
                },
                new { TokenHash = tokenHash },
                splitOn: "Id,Id,Id");

            return resultado.FirstOrDefault();
        }

        /// <summary>
        /// Inserta un nuevo refresh token.
        /// </summary>
        public async Task AgregarAsync(RefreshToken refreshToken)
        {
            const string sql = """
                INSERT INTO RefreshTokens
                    (UsuarioId, TokenHash, FechaExpiracion, FechaCreacion, FechaRevocacion, ReemplazadoPor)
                VALUES
                    (@UsuarioId, @TokenHash, @FechaExpiracion, @FechaCreacion, @FechaRevocacion, @ReemplazadoPor);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """;

            using var conn = _dapper.CrearConexion();
            await conn.OpenAsync();

            var id = await conn.ExecuteScalarAsync<int>(sql, new
            {
                refreshToken.UsuarioId,
                refreshToken.TokenHash,
                refreshToken.FechaExpiracion,
                refreshToken.FechaCreacion,
                refreshToken.FechaRevocacion,
                refreshToken.ReemplazadoPor
            });

            refreshToken.Id = id;
        }

        /// <summary>
        /// Marca un refresh token como revocado, estableciendo la fecha de
        /// revocación y el token reemplazante.
        /// </summary>
        public async Task RevocarAsync(RefreshToken refreshToken, string? reemplazadoPor)
        {
            const string sql = """
                UPDATE RefreshTokens
                SET FechaRevocacion = @FechaRevocacion, ReemplazadoPor = @ReemplazadoPor
                WHERE Id = @Id
                """;

            using var conn = _dapper.CrearConexion();
            await conn.ExecuteAsync(sql, new
            {
                FechaRevocacion = DateTime.UtcNow,
                ReemplazadoPor = reemplazadoPor,
                refreshToken.Id
            });
        }

        /// <summary>
        /// Revoca todos los refresh tokens activos de un usuario.
        /// Equivalente a: buscar los no revocados y setear FechaRevocacion.
        /// </summary>
        public async Task RevocarTodosPorUsuarioAsync(int usuarioId)
        {
            const string sql = """
                UPDATE RefreshTokens
                SET FechaRevocacion = @Ahora
                WHERE UsuarioId = @UsuarioId AND FechaRevocacion IS NULL
                """;

            using var conn = _dapper.CrearConexion();
            await conn.ExecuteAsync(sql, new { Ahora = DateTime.UtcNow, UsuarioId = usuarioId });
        }
    }
}
