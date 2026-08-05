using ServiciosGenerales.Aplicacion.Dtos.Auth;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.Settings;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Auth
{
    public interface ILoginUseCase
    {
        Task<LoginResultDto> Ejecutar(LoginRequestDto request);
    }

    public class LoginUseCase : ILoginUseCase
    {
        private readonly Dominio.Interfaces.IUsuarioRepository _usuarioRepository;
        private readonly IRefreshTokenFactory _refreshTokenFactory;
        private readonly IServerClock _clock;
        private readonly JwtSettings _jwtSettings;
        private readonly AuthSettings _authSettings;

        public LoginUseCase(
            Dominio.Interfaces.IUsuarioRepository usuarioRepository,
            IRefreshTokenFactory refreshTokenFactory,
            IServerClock clock,
            JwtSettings jwtSettings,
            AuthSettings authSettings)
        {
            _usuarioRepository = usuarioRepository;
            _refreshTokenFactory = refreshTokenFactory;
            _clock = clock;
            _jwtSettings = jwtSettings;
            _authSettings = authSettings;
        }

        public async Task<LoginResultDto> Ejecutar(LoginRequestDto request)
        {
            var usuario = await _usuarioRepository.ObtenerPorDocumentoAsync(request.DocumentoIdentidad);

            // Usuario inexistente o sin contraseña: respuesta genérica, sin contar intentos.
            if (usuario == null || usuario.PasswordHash == null)
                return new LoginResultDto { Exito = false, IntentosRestantes = _authSettings.MaxIntentosFallidos };

            // Cuenta temporalmente bloqueada por intentos fallidos.
            if (usuario.BloqueoHasta != null && usuario.BloqueoHasta > _clock.Now)
                return new LoginResultDto
                {
                    Exito = false,
                    CuentaBloqueada = true,
                    BloqueadoHasta = usuario.BloqueoHasta,
                };

            // Usuario deshabilitado: no acumula intentos ni se bloquea.
            if (!usuario.Estado)
                return new LoginResultDto { Exito = false, IntentosRestantes = _authSettings.MaxIntentosFallidos };

            if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
                return await RegistrarIntentoFallidoAsync(usuario);

            // Éxito: se limpian los contadores de bloqueo.
            if (usuario.IntentosFallidos > 0 || usuario.BloqueoHasta != null)
            {
                usuario.IntentosFallidos = 0;
                usuario.BloqueoHasta = null;
                await _usuarioRepository.ActualizarAsync(usuario);
            }

            var refreshToken = await _refreshTokenFactory.CrearYPersistirAsync(usuario.Id);

            return new LoginResultDto
            {
                Exito = true,
                Usuario = new LoginResponseDto
                {
                    Token = string.Empty,
                    RefreshToken = refreshToken,
                    ExpiraEnMinutos = _jwtSettings.ExpireMinutes,
                    NombreCompleto = usuario.NombreCompleto,
                    DocumentoIdentidad = usuario.DocumentoIdentidad,
                    TipoUsuarioId = usuario.TipoUsuarioId,
                    RolId = usuario.RolId,
                    RolNombre = usuario.Rol?.Nombre ?? "",
                    FotoUrl = usuario.FotoUrl,
                },
            };
        }

        private async Task<LoginResultDto> RegistrarIntentoFallidoAsync(Dominio.Entidades.Usuario usuario)
        {
            var fallidos = usuario.IntentosFallidos + 1;
            usuario.IntentosFallidos = fallidos;

            if (fallidos >= _authSettings.MaxIntentosFallidos)
            {
                usuario.BloqueoHasta = _clock.Now.AddMinutes(_authSettings.DuracionBloqueoMinutos);
                usuario.IntentosFallidos = 0;
                await _usuarioRepository.ActualizarAsync(usuario);

                return new LoginResultDto
                {
                    Exito = false,
                    CuentaBloqueada = true,
                    BloqueadoHasta = usuario.BloqueoHasta,
                };
            }

            await _usuarioRepository.ActualizarAsync(usuario);

            return new LoginResultDto
            {
                Exito = false,
                IntentosRestantes = _authSettings.MaxIntentosFallidos - fallidos,
            };
        }
    }
}
