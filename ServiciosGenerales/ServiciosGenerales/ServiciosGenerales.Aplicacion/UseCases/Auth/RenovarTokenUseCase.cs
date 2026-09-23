using ServiciosGenerales.Aplicacion.Dtos.Auth;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.Settings;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Auth
{
    public interface IRenovarTokenUseCase
    {
        Task<LoginResponseDto?> Ejecutar(string refreshToken);
    }

    /// <summary>
    /// Renueva la sesión: valida el refresh token (no revocado ni expirado),
    /// rota el token (el anterior queda revocado) y emite uno nuevo.
    /// Devuelve null si el refresh token no es válido.
    /// </summary>
    public class RenovarTokenUseCase : IRenovarTokenUseCase
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRefreshTokenFactory _refreshTokenFactory;
        private readonly Dominio.Interfaces.IUsuarioRepository _usuarioRepository;
        private readonly JwtSettings _jwtSettings;

        public RenovarTokenUseCase(
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenService refreshTokenService,
            IRefreshTokenFactory refreshTokenFactory,
            Dominio.Interfaces.IUsuarioRepository usuarioRepository,
            JwtSettings jwtSettings)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenService = refreshTokenService;
            _refreshTokenFactory = refreshTokenFactory;
            _usuarioRepository = usuarioRepository;
            _jwtSettings = jwtSettings;
        }

        public async Task<LoginResponseDto?> Ejecutar(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var hash = _refreshTokenService.ObtenerHash(refreshToken);
            var existente = await _refreshTokenRepository.ObtenerPorHashAsync(hash);
            if (existente == null)
                return null;

            if (!existente.EsValido)
                return null;

            var usuario = existente.Usuario ?? await _usuarioRepository.ObtenerPorIdAsync(existente.UsuarioId);
            if (usuario == null || !usuario.Estado)
                return null;

            // Rotación: el token usado queda revocado y se emite uno nuevo.
            var nuevoTokenEnClaro = await _refreshTokenFactory.CrearYPersistirAsync(usuario.Id);
            await _refreshTokenRepository.RevocarAsync(
                existente,
                _refreshTokenService.ObtenerHash(nuevoTokenEnClaro));

            return new LoginResponseDto
            {
                Token = string.Empty,
                RefreshToken = nuevoTokenEnClaro,
                ExpiraEnMinutos = _jwtSettings.ExpireMinutes,
                NombreCompleto = usuario.NombreCompleto,
                DocumentoIdentidad = usuario.DocumentoIdentidad,
                TipoUsuarioId = usuario.TipoUsuarioId,
                RolId = usuario.RolId,
                RolNombre = usuario.Rol?.Nombre ?? "",
                FotoUrl = usuario.FotoUrl,
            };
        }
    }
}
