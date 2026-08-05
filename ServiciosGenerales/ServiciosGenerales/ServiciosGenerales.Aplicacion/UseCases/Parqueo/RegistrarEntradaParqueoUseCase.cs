using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Parqueo
{
    public interface IRegistrarEntradaParqueoUseCase
    {
        Task<int> Ejecutar(RegistrarEntradaParqueoDto dto);
    }

    public class RegistrarEntradaParqueoUseCase : IRegistrarEntradaParqueoUseCase
    {
        private readonly IRegistroParqueoRepository _repository;
        private readonly IServerClock _clock;

        public RegistrarEntradaParqueoUseCase(IRegistroParqueoRepository repository, IServerClock clock)
        {
            _repository = repository;
            _clock = clock;
        }

        public async Task<int> Ejecutar(RegistrarEntradaParqueoDto dto)
        {
            var registro = new RegistroParqueo
            {
                VehiculoId = dto.VehiculoId,
                PuertaAcceso = dto.PuertaAcceso,
                Observaciones = dto.Observaciones,
                FechaIngreso = _clock.Now,
            };

            return await _repository.RegistrarEntradaAsync(registro);
        }
    }
}
