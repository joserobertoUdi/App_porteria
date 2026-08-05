using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiciosGenerales.Aplicacion.Dtos.Comunes;
using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Aplicacion.UseCases.Porteria;

namespace ServiciosGenerales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,PorteroPorteria")]
    [EnableRateLimiting("global")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public class PorteriaController : ControllerBase
    {
        private readonly IRegistrarEntradaUseCase _registrarEntradaUseCase;
        private readonly IRegistrarEntradaPorteriaCompletaUseCase _registrarEntradaCompletaUseCase;
        private readonly IRegistrarSalidaUseCase _registrarSalidaUseCase;
        private readonly IObtenerVisitasActivasUseCase _obtenerVisitasActivasUseCase;

        public PorteriaController(
            IRegistrarEntradaUseCase registrarEntradaUseCase,
            IRegistrarEntradaPorteriaCompletaUseCase registrarEntradaCompletaUseCase,
            IRegistrarSalidaUseCase registrarSalidaUseCase,
            IObtenerVisitasActivasUseCase obtenerVisitasActivasUseCase)
        {
            _registrarEntradaUseCase = registrarEntradaUseCase;
            _registrarEntradaCompletaUseCase = registrarEntradaCompletaUseCase;
            _registrarSalidaUseCase = registrarSalidaUseCase;
            _obtenerVisitasActivasUseCase = obtenerVisitasActivasUseCase;
        }

        /// <summary>
        /// Registra la entrada de una visita por la portería.
        /// </summary>
        /// <param name="dto">Datos de la entrada (documento y motivo de visita).</param>
        /// <returns>Identificador del registro de entrada creado.</returns>
        [HttpPost("entrada")]
        [ProducesResponseType(typeof(RegistroCreadoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarEntradaDto dto)
        {
            var idNuevoRegistro = await _registrarEntradaUseCase.Ejecutar(dto);
            return Ok(new { id = idNuevoRegistro, mensaje = "Entrada registrada exitosamente" });
        }

        /// <summary>
        /// Registra la entrada de una visita creando automáticamente el usuario si no existe.
        /// </summary>
        /// <param name="dto">Datos completos de la entrada (visitante y visita).</param>
        /// <returns>Registro creado e indicador de si se creó el usuario.</returns>
        [HttpPost("entrada-completa")]
        [ProducesResponseType(typeof(EntradaPorteriaCompletaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarEntradaCompleta([FromBody] RegistrarEntradaPorteriaCompletaDto dto)
        {
            var resultado = await _registrarEntradaCompletaUseCase.Ejecutar(dto);
            return Ok(new
            {
                id = resultado.RegistroId,
                usuarioId = resultado.UsuarioId,
                usuarioCreado = resultado.UsuarioCreado,
                mensaje = "Entrada registrada exitosamente"
            });
        }

        /// <summary>
        /// Registra la salida de una visita previamente registrada en la portería.
        /// </summary>
        /// <param name="dto">Identificador del registro de entrada a cerrar.</param>
        /// <returns>Mensaje de confirmación o error si no se encontró el registro.</returns>
        [HttpPost("salida")]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarSalida([FromBody] RegistrarSalidaDto dto)
        {
            var resultado = await _registrarSalidaUseCase.Ejecutar(dto);
            if (resultado)
                return Ok(new { mensaje = "Salida registrada exitosamente" });

            return NotFound(new { mensaje = "No se encontró el registro o no se pudo actualizar" });
        }

        /// <summary>
        /// Obtiene la lista de visitas actualmente activas en la portería.
        /// </summary>
        /// <returns>Lista de visitas activas.</returns>
        [HttpGet("activos")]
        [ProducesResponseType(typeof(IEnumerable<VisitaActivaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerActivos()
        {
            var lista = await _obtenerVisitasActivasUseCase.Ejecutar();
            return Ok(lista);
        }
    }
}
