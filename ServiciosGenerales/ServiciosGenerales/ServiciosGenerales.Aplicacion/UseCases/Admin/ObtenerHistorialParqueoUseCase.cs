using ServiciosGenerales.Aplicacion.Dtos.Admin;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface IObtenerHistorialParqueoUseCase
    {
        Task<IEnumerable<HistorialParqueoDto>> Ejecutar();
    }

    public class ObtenerHistorialParqueoUseCase : IObtenerHistorialParqueoUseCase
    {
        private readonly IRegistroParqueoRepository _repository;

        public ObtenerHistorialParqueoUseCase(IRegistroParqueoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<HistorialParqueoDto>> Ejecutar()
        {
            var registros = await _repository.ObtenerHistorialAsync();

            return registros.Select(r => new HistorialParqueoDto
            {
                IdRegistro = r.Id,
                Matricula = r.Vehiculo?.Matricula ?? "",
                Marca = r.Vehiculo?.Marca,
                Modelo = r.Vehiculo?.Modelo,
                Color = r.Vehiculo?.Color,
                NombreCompleto = r.Vehiculo?.Usuario?.NombreCompleto ?? "",
                DocumentoIdentidad = r.Vehiculo?.Usuario?.DocumentoIdentidad ?? "",
                FechaIngreso = r.FechaIngreso,
                FechaSalida = r.FechaSalida,
                PuertaAcceso = r.PuertaAcceso,
                Observaciones = r.Observaciones,
                RegistradoPorNombre = r.RegistradoPor?.NombreCompleto,
                RegistradoPorDocumento = r.RegistradoPor?.DocumentoIdentidad,
            });
        }
    }
}
