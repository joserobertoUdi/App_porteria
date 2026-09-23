using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Dominio.Interfaces
{
    public interface IVehiculoRepository
    {
        Task<Vehiculo?> ObtenerPorIdAsync(int id);
        Task<Vehiculo?> ObtenerPorMatriculaAsync(string matricula);
        Task<IEnumerable<Vehiculo>> ObtenerPorUsuarioIdAsync(int usuarioId);
        Task<int> CrearAsync(Vehiculo vehiculo);
        Task<bool> ActualizarAsync(Vehiculo vehiculo);
    }
}
