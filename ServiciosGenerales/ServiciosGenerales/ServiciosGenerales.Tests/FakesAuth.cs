using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Tests
{
    /// <summary>Servicio de refresh tokens determinista para pruebas.</summary>
    public sealed class FakeRefreshTokenService : IRefreshTokenService
    {
        private int _contador;

        public string GenerarToken() => $"RT-{++_contador}";

        public string ObtenerHash(string token) => $"HASH-{token}";
    }

    /// <summary>Repositorio de refresh tokens en memoria.</summary>
    public sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly List<RefreshToken> _tokens = new();
        public IReadOnlyList<RefreshToken> Tokens => _tokens;

        public Task<RefreshToken?> ObtenerPorHashAsync(string tokenHash)
            => Task.FromResult(_tokens.FirstOrDefault(t => t.TokenHash == tokenHash));

        public Task AgregarAsync(RefreshToken refreshToken)
        {
            refreshToken.Id = _tokens.Count + 1;
            _tokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task RevocarAsync(RefreshToken refreshToken, string? reemplazadoPor)
        {
            refreshToken.FechaRevocacion = DateTime.UtcNow;
            refreshToken.ReemplazadoPor = reemplazadoPor;
            return Task.CompletedTask;
        }

        public Task RevocarTodosPorUsuarioAsync(int usuarioId)
        {
            foreach (var t in _tokens.Where(t => t.UsuarioId == usuarioId && t.FechaRevocacion == null))
                t.FechaRevocacion = DateTime.UtcNow;
            return Task.CompletedTask;
        }
    }

    /// <summary>Repositorio de usuarios configurable para pruebas.</summary>
    public sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly List<Usuario> _usuarios = new();

        public FakeUsuarioRepository(params Usuario[] usuarios)
        {
            _usuarios.AddRange(usuarios);
        }

        public IReadOnlyList<Usuario> Usuarios => _usuarios;

        public IEnumerable<Usuario> _ObtenerTodos() => _usuarios;

        public Task<Usuario?> ObtenerPorDocumentoAsync(string documento)
            => Task.FromResult(_usuarios.FirstOrDefault(u => u.DocumentoIdentidad == documento));

        public Task<Usuario?> ObtenerPorIdAsync(int id)
            => Task.FromResult(_usuarios.FirstOrDefault(u => u.Id == id));

        public Task<IEnumerable<Usuario>> ObtenerCoincidenciaAsync(string termino)
            => Task.FromResult<IEnumerable<Usuario>>(new List<Usuario>());

        public Task<IEnumerable<Usuario>> ObtenerTodosAsync()
            => Task.FromResult<IEnumerable<Usuario>>(_usuarios);

        public Task<int> CrearAsync(Usuario usuario)
        {
            usuario.Id = _usuarios.Count + 1;
            _usuarios.Add(usuario);
            return Task.FromResult(usuario.Id);
        }

        public Task<bool> ActualizarAsync(Usuario usuario)
            => Task.FromResult(true);
    }

    public static class AuthFakes
    {
        public static Usuario CrearUsuarioActivo(int id = 1, string documento = "12345678", bool estado = true)
        {
            return new Usuario
            {
                Id = id,
                NombreCompleto = "Usuario de prueba",
                DocumentoIdentidad = documento,
                TipoUsuarioId = 1,
                RolId = 1,
                Rol = new Rol { Id = 1, Nombre = "Administrador" },
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("clave123"),
                Estado = estado,
            };
        }

        public static ServiciosGenerales.Aplicacion.Settings.JwtSettings JwtSettings =>
            new()
            {
                Key = "ClaveDePruebaParaJwt-2026-0123456789ABCDEF",
                Issuer = "UDI",
                Audience = "app-universidad",
                ExpireMinutes = 180,
                RefreshTokenExpireDays = 7,
            };

        public static ServiciosGenerales.Aplicacion.Settings.AuthSettings AuthSettings =>
            new()
            {
                MaxIntentosFallidos = 5,
                DuracionBloqueoMinutos = 15,
            };
    }
}
