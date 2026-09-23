using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.UseCases.Usuarios;

namespace ServiciosGenerales.Aplicacion.UseCases.Porteria
{
    public interface IRegistrarEntradaPorteriaCompletaUseCase
    {
        Task<ResultadoEntradaPorteria> Ejecutar(RegistrarEntradaPorteriaCompletaDto dto, int? registradoPorUsuarioId);

        /// <summary>
        /// Registra entrada con foto adjunta en el mismo proceso.
        /// La foto se sube al servicio documental y la referencia se guarda
        /// en el registro, todo dentro de la misma operación.
        /// </summary>
        Task<ResultadoEntradaPorteria> EjecutarConFoto(
            RegistrarEntradaPorteriaCompletaDto dto,
            Stream? fotoStream,
            string? nombreArchivo,
            string usuarioRegistro,
            int? registradoPorUsuarioId);
    }

    /// <summary>
    /// Registra la entrada a portería de una persona, buscando o creando su usuario automáticamente.
    /// Los usuarios creados por este flujo NO tienen credenciales y NO pueden iniciar sesión.
    /// </summary>
    public class RegistrarEntradaPorteriaCompletaUseCase : IRegistrarEntradaPorteriaCompletaUseCase
    {
        private readonly IBuscarOCrearUsuarioPorCarnetUseCase _buscarOCrearUsuario;
        private readonly IRegistrarEntradaUseCase _registrarEntrada;
        private readonly ISharepointService _sharepointService;

        public RegistrarEntradaPorteriaCompletaUseCase(
            IBuscarOCrearUsuarioPorCarnetUseCase buscarOCrearUsuario,
            IRegistrarEntradaUseCase registrarEntrada,
            ISharepointService sharepointService)
        {
            _buscarOCrearUsuario = buscarOCrearUsuario;
            _registrarEntrada = registrarEntrada;
            _sharepointService = sharepointService;
        }

        public async Task<ResultadoEntradaPorteria> Ejecutar(RegistrarEntradaPorteriaCompletaDto dto, int? registradoPorUsuarioId)
        {
            var resultadoUsuario = await _buscarOCrearUsuario.Ejecutar(
                dto.DocumentoIdentidad,
                dto.NombreCompleto,
                dto.TipoUsuarioId);

            var registroId = await _registrarEntrada.Ejecutar(new RegistrarEntradaDto
            {
                UsuarioId = resultadoUsuario.Usuario.Id,
                RegistradoPorUsuarioId = registradoPorUsuarioId,
                PuertaEntrada = dto.PuertaEntrada,
                MotivoVisita = dto.MotivoVisita,
                AreaDestino = dto.AreaDestino,
                FotoUrl = dto.FotoUrl,
            });

            return new ResultadoEntradaPorteria
            {
                RegistroId = registroId,
                UsuarioId = resultadoUsuario.Usuario.Id,
                UsuarioCreado = resultadoUsuario.Creado,
            };
        }

        public async Task<ResultadoEntradaPorteria> EjecutarConFoto(
            RegistrarEntradaPorteriaCompletaDto dto,
            Stream? fotoStream,
            string? nombreArchivo,
            string usuarioRegistro,
            int? registradoPorUsuarioId)
        {
            var resultadoUsuario = await _buscarOCrearUsuario.Ejecutar(
                dto.DocumentoIdentidad,
                dto.NombreCompleto,
                dto.TipoUsuarioId);

            // Si hay foto, subirla al servicio documental ANTES de guardar el registro.
            string? fotoUrl = dto.FotoUrl;
            if (fotoStream != null && fotoStream.Length > 0 && !string.IsNullOrEmpty(nombreArchivo))
            {
                var subida = await _sharepointService.SubirArchivoAsync(
                    fotoStream,
                    nombreArchivo,
                    dto.DocumentoIdentidad,
                    usuarioRegistro);

                if (subida != null)
                {
                    fotoUrl = subida.Referencia;
                }
            }

            var registroId = await _registrarEntrada.Ejecutar(new RegistrarEntradaDto
            {
                UsuarioId = resultadoUsuario.Usuario.Id,
                RegistradoPorUsuarioId = registradoPorUsuarioId,
                PuertaEntrada = dto.PuertaEntrada,
                MotivoVisita = dto.MotivoVisita,
                AreaDestino = dto.AreaDestino,
                FotoUrl = fotoUrl,
            });

            return new ResultadoEntradaPorteria
            {
                RegistroId = registroId,
                UsuarioId = resultadoUsuario.Usuario.Id,
                UsuarioCreado = resultadoUsuario.Creado,
            };
        }
    }
}
