using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Parqueo
{
    public interface IListarVehiculosPorUsuarioUseCase
    {
        Task<IEnumerable<VehiculoDto>> Ejecutar(int usuarioId);
    }

    public class ListarVehiculosPorUsuarioUseCase : IListarVehiculosPorUsuarioUseCase
    {
        private readonly IVehiculoRepository _repository;

        public ListarVehiculosPorUsuarioUseCase(IVehiculoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<VehiculoDto>> Ejecutar(int usuarioId)
        {
            var vehiculos = await _repository.ObtenerPorUsuarioIdAsync(usuarioId);

            return vehiculos.Select(v => new VehiculoDto
            {
                Id = v.Id,
                UsuarioId = v.UsuarioId,
                Matricula = v.Matricula,
                Marca = v.Marca,
                Modelo = v.Modelo,
                Color = v.Color,
                Activo = v.Activo,
                FechaRegistro = v.FechaRegistro,
            });
        }
    }
}
