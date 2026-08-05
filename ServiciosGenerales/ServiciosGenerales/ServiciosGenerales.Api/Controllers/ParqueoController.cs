using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiciosGenerales.Aplicacion.Dtos.Comunes;
using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Aplicacion.UseCases.Parqueo;

namespace ServiciosGenerales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,PorteroParqueo")]
    [EnableRateLimiting("global")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public class ParqueoController : ControllerBase
    {
        private readonly IBuscarVehiculoPorPlacaUseCase _buscarVehiculoPorPlacaUseCase;
        private readonly IListarVehiculosPorUsuarioUseCase _listarVehiculosUseCase;
        private readonly IRegistrarEntradaParqueoUseCase _registrarEntradaUseCase;
        private readonly IRegistrarSalidaParqueoUseCase _registrarSalidaUseCase;
        private readonly IObtenerParqueosActivosUseCase _obtenerActivosUseCase;
        private readonly IObtenerHistorialParqueoUseCase _obtenerHistorialUseCase;
        private readonly IRegistrarEntradaParqueoCompletaUseCase _entradaCompletaUseCase;

        public ParqueoController(
            IBuscarVehiculoPorPlacaUseCase buscarVehiculoPorPlacaUseCase,
            IListarVehiculosPorUsuarioUseCase listarVehiculosUseCase,
            IRegistrarEntradaParqueoUseCase registrarEntradaUseCase,
            IRegistrarSalidaParqueoUseCase registrarSalidaUseCase,
            IObtenerParqueosActivosUseCase obtenerActivosUseCase,
            IObtenerHistorialParqueoUseCase obtenerHistorialUseCase,
            IRegistrarEntradaParqueoCompletaUseCase entradaCompletaUseCase)
        {
            _buscarVehiculoPorPlacaUseCase = buscarVehiculoPorPlacaUseCase;
            _listarVehiculosUseCase = listarVehiculosUseCase;
            _registrarEntradaUseCase = registrarEntradaUseCase;
            _registrarSalidaUseCase = registrarSalidaUseCase;
            _obtenerActivosUseCase = obtenerActivosUseCase;
            _obtenerHistorialUseCase = obtenerHistorialUseCase;
            _entradaCompletaUseCase = entradaCompletaUseCase;
        }

        /// <summary>
        /// Busca un vehículo por su placa/matrícula.
        /// </summary>
        /// <param name="placa">Placa o matrícula del vehículo.</param>
        /// <returns>Datos del vehículo encontrado.</returns>
        [HttpGet("vehiculos/buscar-placa/{placa}")]
        [ProducesResponseType(typeof(VehiculoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarVehiculoPorPlaca(string placa)
        {
            var vehiculo = await _buscarVehiculoPorPlacaUseCase.Ejecutar(placa);
            if (vehiculo == null)
                return NotFound(new { mensaje = "Vehículo no encontrado" });
            return Ok(vehiculo);
        }

        /// <summary>
        /// Lista los vehículos registrados de un usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <returns>Lista de vehículos del usuario.</returns>
        [HttpGet("vehiculos/{usuarioId}")]
        [ProducesResponseType(typeof(IEnumerable<VehiculoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarVehiculos(int usuarioId)
        {
            var vehiculos = await _listarVehiculosUseCase.Ejecutar(usuarioId);
            return Ok(vehiculos);
        }

        /// <summary>
        /// Registra la entrada de un vehículo al parqueo creando automáticamente el usuario y el vehículo si no existen.
        /// </summary>
        /// <param name="dto">Datos completos de la entrada (vehículo, usuario y acceso).</param>
        /// <returns>Registro creado e indicadores de si se creó el usuario y el vehículo.</returns>
        [HttpPost("entrada-completa")]
        [ProducesResponseType(typeof(EntradaParqueoCompletaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarEntradaCompleta([FromBody] RegistrarEntradaParqueoCompletaDto dto)
        {
            var resultado = await _entradaCompletaUseCase.Ejecutar(dto);
            return Ok(new
            {
                id = resultado.RegistroId,
                usuarioId = resultado.UsuarioId,
                vehiculoId = resultado.VehiculoId,
                usuarioCreado = resultado.UsuarioCreado,
                vehiculoCreado = resultado.VehiculoCreado,
                mensaje = "Entrada de parqueo registrada"
            });
        }

        /// <summary>
        /// Registra la entrada de un vehículo al parqueo.
        /// </summary>
        /// <param name="dto">Datos de la entrada (placa, usuario y puerta de acceso).</param>
        /// <returns>Identificador del registro de entrada creado.</returns>
        [HttpPost("entrada")]
        [ProducesResponseType(typeof(RegistroCreadoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarEntradaParqueoDto dto)
        {
            var id = await _registrarEntradaUseCase.Ejecutar(dto);
            return Ok(new { id, mensaje = "Entrada de parqueo registrada" });
        }

        /// <summary>
        /// Registra la salida de un vehículo del parqueo.
        /// </summary>
        /// <param name="dto">Identificador del registro de entrada a cerrar.</param>
        /// <returns>Mensaje de confirmación o error si no se encontró el registro.</returns>
        [HttpPost("salida")]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarSalida([FromBody] RegistrarSalidaParqueoDto dto)
        {
            var resultado = await _registrarSalidaUseCase.Ejecutar(dto);
            if (resultado)
                return Ok(new { mensaje = "Salida de parqueo registrada" });
            return NotFound(new { mensaje = "Registro no encontrado" });
        }

        /// <summary>
        /// Obtiene la lista de vehículos actualmente estacionados.
        /// </summary>
        /// <returns>Lista de parqueos activos.</returns>
        [HttpGet("activos")]
        [ProducesResponseType(typeof(IEnumerable<ParqueoActivoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerActivos()
        {
            var lista = await _obtenerActivosUseCase.Ejecutar();
            return Ok(lista);
        }

        /// <summary>
        /// Obtiene el historial de parqueo de un usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <returns>Historial de entradas de parqueo del usuario.</returns>
        [HttpGet("historial/{usuarioId}")]
        [ProducesResponseType(typeof(IEnumerable<ParqueoActivoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerHistorial(int usuarioId)
        {
            var lista = await _obtenerHistorialUseCase.EjecutarPorUsuario(usuarioId);
            return Ok(lista);
        }
    }
}
