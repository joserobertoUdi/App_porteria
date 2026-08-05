using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Parqueo
{
    public interface IBuscarOCrearVehiculoUseCase
    {
        Task<ResultadoBuscarOCrearVehiculo> Ejecutar(int usuarioId, string matricula, string? marca, string? modelo, string? color);
    }

    /// <summary>
    /// Busca un vehículo por matrícula; si no existe, lo crea asociado al usuario indicado.
    /// </summary>
    public class BuscarOCrearVehiculoUseCase : IBuscarOCrearVehiculoUseCase
    {
        private readonly IVehiculoRepository _repository;

        public BuscarOCrearVehiculoUseCase(IVehiculoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResultadoBuscarOCrearVehiculo> Ejecutar(int usuarioId, string matricula, string? marca, string? modelo, string? color)
        {
            var placa = matricula.Trim().ToUpper();
            var vehiculo = await _repository.ObtenerPorMatriculaAsync(placa);

            if (vehiculo != null)
            {
                return new ResultadoBuscarOCrearVehiculo { Vehiculo = vehiculo, Creado = false };
            }

            vehiculo = new Vehiculo
            {
                UsuarioId = usuarioId,
                Matricula = placa,
                Marca = marca,
                Modelo = modelo,
                Color = color,
                Activo = true,
            };

            await _repository.CrearAsync(vehiculo);
            return new ResultadoBuscarOCrearVehiculo { Vehiculo = vehiculo, Creado = true };
        }
    }
}
