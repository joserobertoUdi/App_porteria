using Microsoft.EntityFrameworkCore;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    public class RegistroParqueoRepository : IRegistroParqueoRepository
    {
        private readonly UniversidadDbContext _context;

        public RegistroParqueoRepository(UniversidadDbContext context)
        {
            _context = context;
        }

        public async Task<int> RegistrarEntradaAsync(RegistroParqueo registro)
        {
            _context.RegistrosParqueo.Add(registro);
            await _context.SaveChangesAsync();
            return registro.Id;
        }

        public async Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida)
        {
            var registro = await _context.RegistrosParqueo.FindAsync(id);
            if (registro == null) return false;

            registro.FechaSalida = fechaSalida;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<RegistroParqueo>> ObtenerActivosAsync()
        {
            return await _context.RegistrosParqueo
                .Where(r => r.FechaSalida == null)
                .Include(r => r.Vehiculo)
                    .ThenInclude(v => v!.Usuario)
                .OrderByDescending(r => r.FechaIngreso)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegistroParqueo>> ObtenerPorVehiculoIdAsync(int vehiculoId)
        {
            return await _context.RegistrosParqueo
                .Where(r => r.VehiculoId == vehiculoId)
                .Include(r => r.Vehiculo)
                .OrderByDescending(r => r.FechaIngreso)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegistroParqueo>> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            return await _context.RegistrosParqueo
                .Where(r => r.Vehiculo != null && r.Vehiculo.UsuarioId == usuarioId)
                .Include(r => r.Vehiculo)
                    .ThenInclude(v => v!.Usuario)
                .OrderByDescending(r => r.FechaIngreso)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegistroParqueo>> ObtenerHistorialAsync()
        {
            return await _context.RegistrosParqueo
                .Include(r => r.Vehiculo)
                    .ThenInclude(v => v!.Usuario)
                        .ThenInclude(u => u!.TipoUsuario)
                .OrderByDescending(r => r.FechaIngreso)
                .ToListAsync();
        }
    }
}
