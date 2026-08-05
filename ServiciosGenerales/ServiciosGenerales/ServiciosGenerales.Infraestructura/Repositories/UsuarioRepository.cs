using Microsoft.EntityFrameworkCore;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly UniversidadDbContext _context;

        public UsuarioRepository(UniversidadDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorDocumentoAsync(string documento)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.DocumentoIdentidad == documento);
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<Usuario>> ObtenerCoincidenciaAsync(string termino)
        {
            return await _context.Usuarios
                .Where(u => u.Estado &&
                    (u.DocumentoIdentidad.StartsWith(termino) ||
                     u.NombreCompleto.Contains(termino)))
                .OrderBy(u => u.NombreCompleto)
                .Take(5)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.TipoUsuario)
                .OrderByDescending(u => u.FechaRegistro)
                .ToListAsync();
        }

        public async Task<int> CrearAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario.Id;
        }

        public async Task<bool> ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
