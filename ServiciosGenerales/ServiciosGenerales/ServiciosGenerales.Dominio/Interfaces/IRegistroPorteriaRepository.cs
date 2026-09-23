using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Dominio.Interfaces
{
    public interface IRegistroPorteriaRepository
    {
        Task<int> RegistrarEntradaAsync(RegistroPorteria registro);
        Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida, string? puertaSalida);
        Task<IEnumerable<RegistroPorteria>> ObtenerVisitasActivasAsync();
        Task<IEnumerable<RegistroPorteria>> ObtenerHistorialAsync();
        Task<IEnumerable<RegistroPorteria>> ObtenerPorUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<RegistroPorteria>> ObtenerPorRegistradoPorIdAsync(int registradoPorUsuarioId);
    }
}
