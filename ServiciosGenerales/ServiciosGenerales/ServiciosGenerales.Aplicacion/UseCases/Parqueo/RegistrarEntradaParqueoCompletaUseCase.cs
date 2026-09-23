using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Aplicacion.UseCases.Usuarios;

namespace ServiciosGenerales.Aplicacion.UseCases.Parqueo
{
    public interface IRegistrarEntradaParqueoCompletaUseCase
    {
        Task<EntradaCompletaResultado> Ejecutar(RegistrarEntradaParqueoCompletaDto dto, int? registradoPorUsuarioId);
    }

    /// <summary>
    /// Orquesta la entrada completa de parqueo: busca o crea el usuario (sin credenciales),
    /// busca o crea el vehículo y registra la entrada. No es un "god class": cada paso
    /// se delega a un caso de uso de responsabilidad única.
    /// </summary>
    public class RegistrarEntradaParqueoCompletaUseCase : IRegistrarEntradaParqueoCompletaUseCase
    {
        private readonly IBuscarOCrearUsuarioPorCarnetUseCase _buscarOCrearUsuario;
        private readonly IBuscarOCrearVehiculoUseCase _buscarOCrearVehiculo;
        private readonly IRegistrarEntradaParqueoUseCase _registrarEntrada;

        public RegistrarEntradaParqueoCompletaUseCase(
            IBuscarOCrearUsuarioPorCarnetUseCase buscarOCrearUsuario,
            IBuscarOCrearVehiculoUseCase buscarOCrearVehiculo,
            IRegistrarEntradaParqueoUseCase registrarEntrada)
        {
            _buscarOCrearUsuario = buscarOCrearUsuario;
            _buscarOCrearVehiculo = buscarOCrearVehiculo;
            _registrarEntrada = registrarEntrada;
        }

        public async Task<EntradaCompletaResultado> Ejecutar(RegistrarEntradaParqueoCompletaDto dto, int? registradoPorUsuarioId)
        {
            var tipoUsuarioId = int.TryParse(dto.TipoUsuarioId, out var tipoId) ? tipoId : 3;

            var resultadoUsuario = await _buscarOCrearUsuario.Ejecutar(
                dto.DocumentoIdentidad,
                dto.NombreCompleto,
                tipoUsuarioId);

            var resultadoVehiculo = await _buscarOCrearVehiculo.Ejecutar(
                resultadoUsuario.Usuario.Id,
                dto.Matricula,
                dto.Marca,
                dto.Modelo,
                dto.Color);

            var registroId = await _registrarEntrada.Ejecutar(new RegistrarEntradaParqueoDto
            {
                VehiculoId = resultadoVehiculo.Vehiculo.Id,
                RegistradoPorUsuarioId = registradoPorUsuarioId,
                PuertaAcceso = dto.PuertaAcceso,
                Observaciones = dto.Observaciones,
            });

            return new EntradaCompletaResultado
            {
                RegistroId = registroId,
                UsuarioId = resultadoUsuario.Usuario.Id,
                VehiculoId = resultadoVehiculo.Vehiculo.Id,
                UsuarioCreado = resultadoUsuario.Creado,
                VehiculoCreado = resultadoVehiculo.Creado,
            };
        }
    }
}
