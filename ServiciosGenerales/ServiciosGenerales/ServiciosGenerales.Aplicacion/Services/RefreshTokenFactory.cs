using ServiciosGenerales.Aplicacion.Settings;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.Services
{
    public interface IRefreshTokenFactory
    {
        /// <summary>Genera, persiste (hasheado) y devuelve en claro un nuevo refresh token.</summary>
        Task<string> CrearYPersistirAsync(int usuarioId);
    }

    public class RefreshTokenFactory : IRefreshTokenFactory
    {
        private readonly IRefreshTokenRepository _repository;
        private readonly IRefreshTokenService _service;
        private readonly IServerClock _clock;
        private readonly JwtSettings _jwtSettings;

        public RefreshTokenFactory(
            IRefreshTokenRepository repository,
            IRefreshTokenService service,
            IServerClock clock,
            JwtSettings jwtSettings)
        {
            _repository = repository;
            _service = service;
            _clock = clock;
            _jwtSettings = jwtSettings;
        }

        public async Task<string> CrearYPersistirAsync(int usuarioId)
        {
            var tokenEnClaro = _service.GenerarToken();
            var nuevo = new RefreshToken
            {
                UsuarioId = usuarioId,
                TokenHash = _service.ObtenerHash(tokenEnClaro),
                FechaExpiracion = _clock.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
            };
            await _repository.AgregarAsync(nuevo);
            return tokenEnClaro;
        }
    }
}
