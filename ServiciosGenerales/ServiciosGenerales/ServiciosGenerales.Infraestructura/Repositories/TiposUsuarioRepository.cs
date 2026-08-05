using Microsoft.EntityFrameworkCore;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    public class TiposUsuarioRepository : ITiposUsuarioRepository
    {
        private readonly UniversidadDbContext _context;

        public TiposUsuarioRepository(UniversidadDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TiposUsuario>> ObtenerTodosAsync()
        {
            return await _context.TiposUsuarios.OrderBy(t => t.Id).ToListAsync();
        }
    }
}