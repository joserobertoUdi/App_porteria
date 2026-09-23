using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiciosGenerales.Aplicacion.Dtos.Comunes;
using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Aplicacion.UseCases.Porteria;
using ServiciosGenerales.Dominio.Interfaces;

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
        private readonly IUsuarioRepository _usuarioRepository;

        public PorteriaController(
            IRegistrarEntradaUseCase registrarEntradaUseCase,
            IRegistrarEntradaPorteriaCompletaUseCase registrarEntradaCompletaUseCase,
            IRegistrarSalidaUseCase registrarSalidaUseCase,
            IObtenerVisitasActivasUseCase obtenerVisitasActivasUseCase,
            IUsuarioRepository usuarioRepository)
        {
            _registrarEntradaUseCase = registrarEntradaUseCase;
            _registrarEntradaCompletaUseCase = registrarEntradaCompletaUseCase;
            _registrarSalidaUseCase = registrarSalidaUseCase;
            _obtenerVisitasActivasUseCase = obtenerVisitasActivasUseCase;
            _usuarioRepository = usuarioRepository;
        }

        /// <summary>
        /// Resuelve el Id del usuario autenticado a partir del claim
        /// NameIdentifier del JWT (que es el documento de identidad).
        /// Se usa para marcar quién aprueba cada ingreso (auditoría de guardia).
        /// Puede ser null si el claim no existe o el usuario fue eliminado.
        /// </summary>
        private async Task<int?> ObtenerOperadorIdActual()
        {
            var documento = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(documento))
                return null;

            var usuario = await _usuarioRepository.ObtenerPorDocumentoAsync(documento);
            return usuario?.Id;
        }

        /// <summary>
        /// Registra la entrada de una visita por la portería.
        /// </summary>
        [HttpPost("entrada")]
        [ProducesResponseType(typeof(RegistroCreadoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarEntradaDto dto)
        {
            dto.RegistradoPorUsuarioId = await ObtenerOperadorIdActual();
            var idNuevoRegistro = await _registrarEntradaUseCase.Ejecutar(dto);
            return Ok(new { id = idNuevoRegistro, mensaje = "Entrada registrada exitosamente" });
        }

        /// <summary>
        /// Registra la entrada completa (JSON) sin foto adjunta.
        /// La foto, si se necesita, se sube por separado y se pasa como referencia en FotoUrl.
        /// </summary>
        [HttpPost("entrada-completa")]
        [ProducesResponseType(typeof(EntradaPorteriaCompletaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarEntradaCompleta([FromBody] RegistrarEntradaPorteriaCompletaDto dto)
        {
            var resultado = await _registrarEntradaCompletaUseCase.Ejecutar(dto, await ObtenerOperadorIdActual());
            return Ok(new
            {
                id = resultado.RegistroId,
                usuarioId = resultado.UsuarioId,
                usuarioCreado = resultado.UsuarioCreado,
                mensaje = "Entrada registrada exitosamente"
            });
        }

        /// <summary>
        /// Registra la entrada completa con foto adjunta en un solo proceso multipart.
        /// La foto se sube al servicio documental y la referencia se guarda en el
        /// registro, garantizando atomicidad.
        ///
        /// Campos del multipart:
        /// - documentoIdentidad (string, requerido)
        /// - nombreCompleto (string, opcional)
        /// - tipoUsuarioId (int, opcional, default 3)
        /// - puertaEntrada (string, opcional)
        /// - motivoVisita (string, opcional)
        /// - areaDestino (string, opcional)
        /// - foto (archivo, opcional)
        /// </summary>
        [HttpPost("entrada-completa-foto")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(EntradaPorteriaCompletaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegistrarEntradaCompletaConFoto(
            [FromForm] RegistrarEntradaCompletaFormDto formDto)
        {
            var dto = new RegistrarEntradaPorteriaCompletaDto
            {
                DocumentoIdentidad = formDto.DocumentoIdentidad,
                NombreCompleto = formDto.NombreCompleto,
                TipoUsuarioId = formDto.TipoUsuarioId,
                PuertaEntrada = formDto.PuertaEntrada,
                MotivoVisita = formDto.MotivoVisita,
                AreaDestino = formDto.AreaDestino,
            };

            // Obtener el nombre del usuario que registra del token JWT.
            var usuarioRegistro = User.Identity?.Name ?? "porteria";

            Stream? fotoStream = null;
            string? nombreArchivo = null;

            if (formDto.Foto != null && formDto.Foto.Length > 0)
            {
                fotoStream = formDto.Foto.OpenReadStream();
                nombreArchivo = formDto.Foto.FileName;
            }

            try
            {
                var resultado = await _registrarEntradaCompletaUseCase.EjecutarConFoto(
                    dto, fotoStream, nombreArchivo, usuarioRegistro, await ObtenerOperadorIdActual());

                return Ok(new
                {
                    id = resultado.RegistroId,
                    usuarioId = resultado.UsuarioId,
                    usuarioCreado = resultado.UsuarioCreado,
                    mensaje = "Entrada registrada exitosamente"
                });
            }
            finally
            {
                fotoStream?.Dispose();
            }
        }

        /// <summary>
        /// Registra la salida de una visita previamente registrada en la portería.
        /// </summary>
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
        [HttpGet("activos")]
        [ProducesResponseType(typeof(IEnumerable<VisitaActivaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerActivos()
        {
            var lista = await _obtenerVisitasActivasUseCase.Ejecutar();
            return Ok(lista);
        }
    }

    /// <summary>
    /// DTO para el endpoint multipart de entrada completa con foto.
    /// </summary>
    public class RegistrarEntradaCompletaFormDto
    {
        [Microsoft.AspNetCore.Mvc.FromForm(Name = "documentoIdentidad")]
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [System.ComponentModel.DataAnnotations.StringLength(20, MinimumLength = 3)]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "nombreCompleto")]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string? NombreCompleto { get; set; }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "tipoUsuarioId")]
        [System.ComponentModel.DataAnnotations.Range(1, 3)]
        public int TipoUsuarioId { get; set; } = 3;

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "puertaEntrada")]
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string? PuertaEntrada { get; set; }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "motivoVisita")]
        [System.ComponentModel.DataAnnotations.StringLength(500)]
        public string? MotivoVisita { get; set; }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "areaDestino")]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string? AreaDestino { get; set; }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "foto")]
        public IFormFile? Foto { get; set; }
    }
}
