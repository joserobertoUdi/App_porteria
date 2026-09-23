using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Porteria
{
    public interface IObtenerVisitasActivasUseCase
    {
        Task<IEnumerable<VisitaActivaDto>> Ejecutar();
    }

    public class ObtenerVisitasActivasUseCase : IObtenerVisitasActivasUseCase
    {
        private readonly IRegistroPorteriaRepository _repository;

        public ObtenerVisitasActivasUseCase(IRegistroPorteriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<VisitaActivaDto>> Ejecutar()
        {
            var registros = await _repository.ObtenerVisitasActivasAsync();

            return registros.Select(r => new VisitaActivaDto
            {
                IdRegistro = r.Id,
                UsuarioId = r.UsuarioId,
                NombreCompleto = r.Usuario?.NombreCompleto ?? "",
                DocumentoIdentidad = r.Usuario?.DocumentoIdentidad ?? "",
                FechaEntrada = r.FechaEntrada,
                MotivoVisita = r.MotivoVisita,
                PuertaEntrada = r.PuertaEntrada,
                FotoUrl = r.FotoUrl,
            });
        }
    }
}
