using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Dominio.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> ObtenerPorHashAsync(string tokenHash);
        Task AgregarAsync(RefreshToken refreshToken);
        Task RevocarAsync(RefreshToken refreshToken, string? reemplazadoPor);
        Task RevocarTodosPorUsuarioAsync(int usuarioId);
    }
}
