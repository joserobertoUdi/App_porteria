using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Dominio.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorDocumentoAsync(string documento);
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Usuario>> ObtenerCoincidenciaAsync(string termino);
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<int> CrearAsync(Usuario usuario);
        Task<bool> ActualizarAsync(Usuario usuario);
    }
}
