using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiciosGenerales.Aplicacion.Dtos.Sistema;
using ServiciosGenerales.Aplicacion.UseCases.Sistema;

namespace ServiciosGenerales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("global")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public class SistemaController : ControllerBase
    {
        private readonly IObtenerHoraServidorUseCase _obtenerHoraServidor;

        public SistemaController(IObtenerHoraServidorUseCase obtenerHoraServidor)
        {
            _obtenerHoraServidor = obtenerHoraServidor;
        }

        /// <summary>
        /// Obtiene la fecha y hora actuales del servidor con su zona horaria.
        /// </summary>
        /// <returns>Hora del servidor en UTC y local.</returns>
        [HttpGet("hora")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HoraServidorDto), StatusCodes.Status200OK)]
        public IActionResult ObtenerHora()
        {
            return Ok(_obtenerHoraServidor.Ejecutar());
        }
    }
}
