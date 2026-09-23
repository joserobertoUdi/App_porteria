using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Dominio.Interfaces
{
    public interface IRegistroParqueoRepository
    {
        Task<int> RegistrarEntradaAsync(RegistroParqueo registro);
        Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida);
        Task<IEnumerable<RegistroParqueo>> ObtenerActivosAsync();
        Task<IEnumerable<RegistroParqueo>> ObtenerPorVehiculoIdAsync(int vehiculoId);
        Task<IEnumerable<RegistroParqueo>> ObtenerPorUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<RegistroParqueo>> ObtenerPorRegistradoPorIdAsync(int registradoPorUsuarioId);
        Task<IEnumerable<RegistroParqueo>> ObtenerHistorialAsync();
    }
}
