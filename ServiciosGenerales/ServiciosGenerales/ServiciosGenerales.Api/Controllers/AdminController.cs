using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiciosGenerales.Aplicacion.Dtos.Admin;
using ServiciosGenerales.Aplicacion.Dtos.Comunes;
using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Aplicacion.UseCases.Admin;
using ServiciosGenerales.Aplicacion.UseCases.Usuarios;

namespace ServiciosGenerales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [EnableRateLimiting("global")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public class AdminController : ControllerBase
    {
        private readonly IListarUsuariosUseCase _listarUsuarios;
        private readonly ICrearUsuarioUseCase _crearUsuario;
        private readonly IActualizarUsuarioUseCase _actualizarUsuario;
        private readonly ICambiarPasswordUsuarioUseCase _cambiarPassword;
        private readonly IObtenerHistorialPorteriaUseCase _historialPorteria;
        private readonly IObtenerHistorialParqueoUseCase _historialParqueo;
        private readonly IObtenerHistorialUsuarioUseCase _historialUsuario;
        private readonly IObtenerEstadisticasTiposUseCase _estadisticasTipos;
        private readonly IObtenerHistorialTipoUseCase _historialTipo;

        public AdminController(
            IListarUsuariosUseCase listarUsuarios,
            ICrearUsuarioUseCase crearUsuario,
            IActualizarUsuarioUseCase actualizarUsuario,
            ICambiarPasswordUsuarioUseCase cambiarPassword,
            IObtenerHistorialPorteriaUseCase historialPorteria,
            IObtenerHistorialParqueoUseCase historialParqueo,
            IObtenerHistorialUsuarioUseCase historialUsuario,
            IObtenerEstadisticasTiposUseCase estadisticasTipos,
            IObtenerHistorialTipoUseCase historialTipo)
        {
            _listarUsuarios = listarUsuarios;
            _crearUsuario = crearUsuario;
            _actualizarUsuario = actualizarUsuario;
            _cambiarPassword = cambiarPassword;
            _historialPorteria = historialPorteria;
            _historialParqueo = historialParqueo;
            _historialUsuario = historialUsuario;
            _estadisticasTipos = estadisticasTipos;
            _historialTipo = historialTipo;
        }

        /// <summary>
        /// Lista todos los usuarios registrados en el sistema.
        /// </summary>
        /// <returns>Lista de usuarios con su tipo y rol.</returns>
        [HttpGet("usuarios")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioListadoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarUsuarios()
        {
            var lista = await _listarUsuarios.Ejecutar();
            return Ok(lista);
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="dto">Datos del usuario a crear.</param>
        /// <returns>Identificador del usuario creado.</returns>
        [HttpPost("usuarios")]
        [ProducesResponseType(typeof(RegistroCreadoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequestDto dto)
        {
            var id = await _crearUsuario.Ejecutar(dto);
            return Ok(new { id, mensaje = "Usuario creado" });
        }

        /// <summary>
        /// Actualiza la información de un usuario del sistema (nombre, tipo, rol y estado).
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <param name="dto">Datos a actualizar.</param>
        /// <returns>Mensaje de confirmación o 404 si el usuario no existe.</returns>
        [HttpPut("usuarios/{id:int}")]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioRequestDto dto)
        {
            var ok = await _actualizarUsuario.Ejecutar(id, dto);
            if (!ok)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(new { mensaje = "Usuario actualizado" });
        }

        /// <summary>
        /// Restablece la contraseña de un usuario del sistema (caso de contraseña olvidada).
        /// De paso desbloquea la cuenta (limpieza de intentos fallidos y bloqueo temporal).
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <param name="dto">Nueva contraseña.</param>
        /// <returns>Mensaje de confirmación o 404 si el usuario no existe.</returns>
        [HttpPost("usuarios/{id:int}/password")]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordRequestDto dto)
        {
            var ok = await _cambiarPassword.Ejecutar(id, dto);
            if (!ok)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(new { mensaje = "Contraseña restablecida" });
        }

        /// <summary>
        /// Obtiene el historial de visitas de un usuario (portería y parqueo).
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <returns>Historial de portería y parqueo del usuario o 404 si no existe.</returns>
        [HttpGet("usuarios/{id:int}/historial")]
        [ProducesResponseType(typeof(HistorialUsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HistorialUsuario(int id)
        {
            var historial = await _historialUsuario.Ejecutar(id);
            if (historial == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(historial);
        }

        /// <summary>
        /// Estadísticas mensuales de ingresos y salidas (portería y parqueo) por tipo de persona.
        /// </summary>
        /// <param name="anio">Año (por defecto el actual).</param>
        /// <param name="mes">Mes 1-12 (por defecto el actual).</param>
        /// <returns>Conteos por tipo de persona para el mes.</returns>
        [HttpGet("estadisticas/tipos")]
        [ProducesResponseType(typeof(EstadisticasTiposResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> EstadisticasTipos(int? anio, int? mes)
        {
            var ahora = DateTime.Now;
            var resultado = await _estadisticasTipos.Ejecutar(anio ?? ahora.Year, mes ?? ahora.Month);
            return Ok(resultado);
        }

        /// <summary>
        /// Detalle del historial de un tipo de persona (portería y parqueo) en un mes.
        /// </summary>
        /// <param name="tipoUsuarioId">Identificador del tipo de persona.</param>
        /// <param name="anio">Año (por defecto el actual).</param>
        /// <param name="mes">Mes 1-12 (por defecto el actual).</param>
        /// <returns>Historial del tipo de persona o 404 si no existe.</returns>
        [HttpGet("estadisticas/tipos/{tipoUsuarioId:int}/historial")]
        [ProducesResponseType(typeof(HistorialTipoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HistorialTipo(int tipoUsuarioId, int? anio, int? mes)
        {
            var ahora = DateTime.Now;
            var historial = await _historialTipo.Ejecutar(tipoUsuarioId, anio ?? ahora.Year, mes ?? ahora.Month);
            if (historial == null)
                return NotFound(new { mensaje = "Tipo de persona no encontrado" });

            return Ok(historial);
        }

        /// <summary>
        /// Obtiene el historial completo de entradas y salidas por la portería.
        /// </summary>
        /// <returns>Historial de la portería.</returns>
        [HttpGet("porteria/historial")]
        [ProducesResponseType(typeof(IEnumerable<HistorialPorteriaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> HistorialPorteria()
        {
            var lista = await _historialPorteria.Ejecutar();
            return Ok(lista);
        }

        /// <summary>
        /// Obtiene el historial completo de entradas y salidas del parqueo.
        /// </summary>
        /// <returns>Historial del parqueo.</returns>
        [HttpGet("parqueo/historial")]
        [ProducesResponseType(typeof(IEnumerable<HistorialParqueoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> HistorialParqueo()
        {
            var lista = await _historialParqueo.Ejecutar();
            return Ok(lista);
        }
    }
}
