using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Parqueo
{
    public interface IBuscarVehiculoPorPlacaUseCase
    {
        Task<VehiculoDto?> Ejecutar(string placa);
    }

    public class BuscarVehiculoPorPlacaUseCase : IBuscarVehiculoPorPlacaUseCase
    {
        private readonly IVehiculoRepository _repository;

        public BuscarVehiculoPorPlacaUseCase(IVehiculoRepository repository)
        {
            _repository = repository;
        }

        public async Task<VehiculoDto?> Ejecutar(string placa)
        {
            var vehiculo = await _repository.ObtenerPorMatriculaAsync(placa);
            if (vehiculo == null)
                return null;

            return new VehiculoDto
            {
                Id = vehiculo.Id,
                UsuarioId = vehiculo.UsuarioId,
                Matricula = vehiculo.Matricula,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo,
                Color = vehiculo.Color,
                Activo = vehiculo.Activo,
                FechaRegistro = vehiculo.FechaRegistro,
            };
        }
    }
}
