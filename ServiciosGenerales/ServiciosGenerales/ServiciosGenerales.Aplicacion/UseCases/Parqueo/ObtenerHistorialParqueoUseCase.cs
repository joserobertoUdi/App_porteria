using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Parqueo
{
    public interface IObtenerHistorialParqueoUseCase
    {
        Task<IEnumerable<ParqueoActivoDto>> EjecutarPorUsuario(int usuarioId);
    }

    public class ObtenerHistorialParqueoUseCase : IObtenerHistorialParqueoUseCase
    {
        private readonly IRegistroParqueoRepository _repository;

        public ObtenerHistorialParqueoUseCase(IRegistroParqueoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ParqueoActivoDto>> EjecutarPorUsuario(int usuarioId)
        {
            var registros = await _repository.ObtenerPorUsuarioIdAsync(usuarioId);
            return ObtenerParqueosActivosUseCase.Mapear(registros);
        }
    }
}
