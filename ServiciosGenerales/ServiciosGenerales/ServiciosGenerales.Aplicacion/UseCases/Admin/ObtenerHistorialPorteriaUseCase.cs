using ServiciosGenerales.Aplicacion.Dtos.Admin;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface IObtenerHistorialPorteriaUseCase
    {
        Task<IEnumerable<HistorialPorteriaDto>> Ejecutar();
    }

    public class ObtenerHistorialPorteriaUseCase : IObtenerHistorialPorteriaUseCase
    {
        private readonly IRegistroPorteriaRepository _repository;

        public ObtenerHistorialPorteriaUseCase(IRegistroPorteriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<HistorialPorteriaDto>> Ejecutar()
        {
            var registros = await _repository.ObtenerHistorialAsync();

            return registros.Select(r => new HistorialPorteriaDto
            {
                IdRegistro = r.Id,
                NombreCompleto = r.Usuario?.NombreCompleto ?? "",
                DocumentoIdentidad = r.Usuario?.DocumentoIdentidad ?? "",
                FechaEntrada = r.FechaEntrada,
                FechaSalida = r.FechaSalida,
                PuertaEntrada = r.PuertaEntrada,
                PuertaSalida = r.PuertaSalida,
                MotivoVisita = r.MotivoVisita,
                AreaDestino = r.AreaDestino,
            });
        }
    }
}
