using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Porteria
{
    public interface IRegistrarEntradaUseCase
    {
        Task<int> Ejecutar(RegistrarEntradaDto dto);
    }

    public class RegistrarEntradaUseCase : IRegistrarEntradaUseCase
    {
        private readonly IRegistroPorteriaRepository _repository;
        private readonly IServerClock _clock;

        public RegistrarEntradaUseCase(IRegistroPorteriaRepository repository, IServerClock clock)
        {
            _repository = repository;
            _clock = clock;
        }

        public async Task<int> Ejecutar(RegistrarEntradaDto dto)
        {
            var nuevoRegistro = new RegistroPorteria
            {
                UsuarioId = dto.UsuarioId,
                RegistradoPorUsuarioId = dto.RegistradoPorUsuarioId,
                PuertaEntrada = dto.PuertaEntrada,
                MotivoVisita = dto.MotivoVisita,
                AreaDestino = dto.AreaDestino,
                FotoUrl = dto.FotoUrl,
                FechaEntrada = _clock.Now,
            };

            return await _repository.RegistrarEntradaAsync(nuevoRegistro);
        }
    }
}
