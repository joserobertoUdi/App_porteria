using Microsoft.EntityFrameworkCore;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    public class RegistroPorteriaRepository : IRegistroPorteriaRepository
    {
        private readonly UniversidadDbContext _context;

        public RegistroPorteriaRepository(UniversidadDbContext context)
        {
            _context = context;
        }

        public async Task<int> RegistrarEntradaAsync(RegistroPorteria registro)
        {
            _context.RegistrosPorteria.Add(registro);
            await _context.SaveChangesAsync();
            return registro.Id;
        }

        public async Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida, string? puertaSalida)
        {
            var registro = await _context.RegistrosPorteria.FindAsync(id);
            if (registro == null) return false;

            registro.FechaSalida = fechaSalida;
            registro.PuertaSalida = puertaSalida;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<RegistroPorteria>> ObtenerVisitasActivasAsync()
        {
            return await _context.RegistrosPorteria
                .Where(r => r.FechaSalida == null)
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegistroPorteria>> ObtenerHistorialAsync()
        {
            return await _context.RegistrosPorteria
                .Include(r => r.Usuario)
                    .ThenInclude(u => u!.TipoUsuario)
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegistroPorteria>> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            return await _context.RegistrosPorteria
                .Where(r => r.UsuarioId == usuarioId)
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.FechaEntrada)
                .ToListAsync();
        }
    }
}
