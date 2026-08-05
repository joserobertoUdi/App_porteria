using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Aplicacion.UseCases.Usuarios;

namespace ServiciosGenerales.Aplicacion.UseCases.Porteria
{
    public interface IRegistrarEntradaPorteriaCompletaUseCase
    {
        Task<ResultadoEntradaPorteria> Ejecutar(RegistrarEntradaPorteriaCompletaDto dto);
    }

    /// <summary>
    /// Registra la entrada a portería de una persona, buscando o creando su usuario automáticamente.
    /// Los usuarios creados por este flujo NO tienen credenciales y NO pueden iniciar sesión.
    /// </summary>
    public class RegistrarEntradaPorteriaCompletaUseCase : IRegistrarEntradaPorteriaCompletaUseCase
    {
        private readonly IBuscarOCrearUsuarioPorCarnetUseCase _buscarOCrearUsuario;
        private readonly IRegistrarEntradaUseCase _registrarEntrada;

        public RegistrarEntradaPorteriaCompletaUseCase(
            IBuscarOCrearUsuarioPorCarnetUseCase buscarOCrearUsuario,
            IRegistrarEntradaUseCase registrarEntrada)
        {
            _buscarOCrearUsuario = buscarOCrearUsuario;
            _registrarEntrada = registrarEntrada;
        }

        public async Task<ResultadoEntradaPorteria> Ejecutar(RegistrarEntradaPorteriaCompletaDto dto)
        {
            var resultadoUsuario = await _buscarOCrearUsuario.Ejecutar(
                dto.DocumentoIdentidad,
                dto.NombreCompleto,
                dto.TipoUsuarioId);

            var registroId = await _registrarEntrada.Ejecutar(new RegistrarEntradaDto
            {
                UsuarioId = resultadoUsuario.Usuario.Id,
                PuertaEntrada = dto.PuertaEntrada,
                MotivoVisita = dto.MotivoVisita,
                AreaDestino = dto.AreaDestino,
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
