using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiciosGenerales.Aplicacion.Dtos.Comunes;
using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Aplicacion.UseCases.Usuarios;

namespace ServiciosGenerales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("global")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public class UsuarioController : ControllerBase
    {
        private readonly IObtenerUsuarioPorDniUseCase _obtenerUsuarioUseCase;
        private readonly IBuscarUsuariosCoincidentesUseCase _buscarCoincidentes;

        public UsuarioController(
            IObtenerUsuarioPorDniUseCase obtenerUsuarioUseCase,
            IBuscarUsuariosCoincidentesUseCase buscarCoincidentes)
        {
            _obtenerUsuarioUseCase = obtenerUsuarioUseCase;
            _buscarCoincidentes = buscarCoincidentes;
        }

        /// <summary>
        /// Busca un usuario por su documento de identidad.
        /// </summary>
        /// <param name="dni">Documento de identidad del usuario.</param>
        /// <returns>Datos del usuario encontrado.</returns>
        [HttpGet("buscar/{dni}")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarPorDni(string dni)
        {
            var usuario = await _obtenerUsuarioUseCase.Ejecutar(dni);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });
            return Ok(usuario);
        }

        /// <summary>
        /// Sugiere usuarios cuyos datos coincidan parcialmente con el término buscado.
        /// </summary>
        /// <param name="termino">Término de búsqueda (nombre o documento).</param>
        /// <returns>Lista de usuarios coincidentes.</returns>
        [HttpGet("sugerir/{termino}")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SugerirUsuarios(string termino)
        {
            var coincidencias = await _buscarCoincidentes.Ejecutar(termino);
            return Ok(coincidencias);
        }
    }
}
