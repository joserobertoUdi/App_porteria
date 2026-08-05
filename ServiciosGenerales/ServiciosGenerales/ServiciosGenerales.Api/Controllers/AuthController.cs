using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiciosGenerales.Aplicacion.Dtos.Auth;
using ServiciosGenerales.Aplicacion.Dtos.Comunes;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.UseCases.Auth;

namespace ServiciosGenerales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginUseCase _loginUseCase;
        private readonly IRenovarTokenUseCase _renovarTokenUseCase;
        private readonly ITokenService _tokenService;

        public AuthController(
            ILoginUseCase loginUseCase,
            IRenovarTokenUseCase renovarTokenUseCase,
            ITokenService tokenService)
        {
            _loginUseCase = loginUseCase;
            _renovarTokenUseCase = renovarTokenUseCase;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Inicia sesión con las credenciales y devuelve un token JWT para autenticación.
        /// </summary>
        /// <param name="request">Credenciales del usuario.</param>
        /// <returns>Token JWT e información del usuario.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginCredencialesInvalidasResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(LoginBloqueadoResponseDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _loginUseCase.Ejecutar(request);

            if (result.CuentaBloqueada)
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensaje = $"Demasiados intentos fallidos. Cuenta bloqueada temporalmente. " +
                              $"Puede intentar nuevamente a las {result.BloqueadoHasta:HH:mm}.",
                    bloqueadoHasta = result.BloqueadoHasta,
                });

            if (!result.Exito)
                return Unauthorized(new
                {
                    mensaje = result.IntentosRestantes > 0
                        ? $"Credenciales inválidas. Quedan {result.IntentosRestantes} intentos."
                        : "Credenciales inválidas.",
                    intentosRestantes = result.IntentosRestantes,
                });

            result.Usuario!.Token = _tokenService.GenerarToken(result.Usuario!);

            return Ok(result.Usuario);
        }

        /// <summary>
        /// Renueva el token de acceso utilizando un refresh token válido.
        /// </summary>
        /// <param name="request">Refresh token emitido durante el login.</param>
        /// <returns>Nuevo token JWT con el refresh token rotado.</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Refresh([FromBody] RenovarTokenRequestDto request)
        {
            var result = await _renovarTokenUseCase.Ejecutar(request.RefreshToken);
            if (result == null)
                return Unauthorized(new { mensaje = "Sesión expirada. Inicie sesión nuevamente." });

            result.Token = _tokenService.GenerarToken(result);

            return Ok(result);
        }
    }
}
