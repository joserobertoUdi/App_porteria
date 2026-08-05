using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Dominio.Interfaces
{
    public interface ITiposUsuarioRepository
    {
        Task<IEnumerable<TiposUsuario>> ObtenerTodosAsync();
    }
}