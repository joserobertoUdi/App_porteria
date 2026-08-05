using Microsoft.EntityFrameworkCore;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using ServiciosGenerales.Infraestructura.Data;

namespace ServiciosGenerales.Infraestructura.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly UniversidadDbContext _context;

        public RefreshTokenRepository(UniversidadDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> ObtenerPorHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(r => r.Usuario)
                    .ThenInclude(u => u!.Rol)
                .SingleOrDefaultAsync(r => r.TokenHash == tokenHash);
        }

        public async Task AgregarAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevocarAsync(RefreshToken refreshToken, string? reemplazadoPor)
        {
            refreshToken.FechaRevocacion = DateTime.UtcNow;
            refreshToken.ReemplazadoPor = reemplazadoPor;
            await _context.SaveChangesAsync();
        }

        public async Task RevocarTodosPorUsuarioAsync(int usuarioId)
        {
            var activos = await _context.RefreshTokens
                .Where(r => r.UsuarioId == usuarioId && r.FechaRevocacion == null)
                .ToListAsync();

            foreach (var token in activos)
                token.FechaRevocacion = DateTime.UtcNow;

            if (activos.Count > 0)
                await _context.SaveChangesAsync();
        }
    }
}
