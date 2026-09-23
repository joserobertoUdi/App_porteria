using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Porteria
{
    public interface IRegistrarSalidaUseCase
    {
        Task<bool> Ejecutar(RegistrarSalidaDto dto);
    }

    /// <summary>
    /// Registra la salida SIEMPRE con la hora del servidor (IServerClock).
    /// El cliente no puede influir en la fecha registrada.
    /// </summary>
    public class RegistrarSalidaUseCase : IRegistrarSalidaUseCase
    {
        private readonly IRegistroPorteriaRepository _repository;
        private readonly IServerClock _clock;

        public RegistrarSalidaUseCase(IRegistroPorteriaRepository repository, IServerClock clock)
        {
            _repository = repository;
            _clock = clock;
        }

        public async Task<bool> Ejecutar(RegistrarSalidaDto dto)
        {
            return await _repository.RegistrarSalidaAsync(
                dto.Id,
                _clock.Now,
                dto.PuertaSalida);
        }
    }
}
