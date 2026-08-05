using Microsoft.EntityFrameworkCore;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    public class VehiculoRepository : IVehiculoRepository
    {
        private readonly UniversidadDbContext _context;

        public VehiculoRepository(UniversidadDbContext context)
        {
            _context = context;
        }

        public async Task<Vehiculo?> ObtenerPorIdAsync(int id)
        {
            return await _context.Vehiculos.FindAsync(id);
        }

        public async Task<Vehiculo?> ObtenerPorMatriculaAsync(string matricula)
        {
            return await _context.Vehiculos
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.Matricula == matricula);
        }

        public async Task<IEnumerable<Vehiculo>> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            return await _context.Vehiculos
                .Include(v => v.Usuario)
                .Where(v => v.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<int> CrearAsync(Vehiculo vehiculo)
        {
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();
            return vehiculo.Id;
        }

        public async Task<bool> ActualizarAsync(Vehiculo vehiculo)
        {
            _context.Vehiculos.Update(vehiculo);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
