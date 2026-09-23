using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Parqueo
{
    public interface IRegistrarSalidaParqueoUseCase
    {
        Task<bool> Ejecutar(RegistrarSalidaParqueoDto dto);
    }

    public class RegistrarSalidaParqueoUseCase : IRegistrarSalidaParqueoUseCase
    {
        private readonly IRegistroParqueoRepository _repository;
        private readonly IServerClock _clock;

        public RegistrarSalidaParqueoUseCase(IRegistroParqueoRepository repository, IServerClock clock)
        {
            _repository = repository;
            _clock = clock;
        }

        public async Task<bool> Ejecutar(RegistrarSalidaParqueoDto dto)
        {
            return await _repository.RegistrarSalidaAsync(dto.Id, _clock.Now);
        }
    }
}
