using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Parqueo
{
    public interface IObtenerParqueosActivosUseCase
    {
        Task<IEnumerable<ParqueoActivoDto>> Ejecutar();
    }

    public class ObtenerParqueosActivosUseCase : IObtenerParqueosActivosUseCase
    {
        private readonly IRegistroParqueoRepository _repository;

        public ObtenerParqueosActivosUseCase(IRegistroParqueoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ParqueoActivoDto>> Ejecutar()
        {
            var registros = await _repository.ObtenerActivosAsync();
            return Mapear(registros);
        }

        internal static IEnumerable<ParqueoActivoDto> Mapear(IEnumerable<RegistroParqueo> registros)
        {
            return registros.Select(r => new ParqueoActivoDto
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
            });
        }
    }
}
