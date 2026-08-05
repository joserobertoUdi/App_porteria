using System.Security.Cryptography;
using System.Text;

namespace ServiciosGenerales.Aplicacion.Services
{
    public interface IRefreshTokenService
    {
        string GenerarToken();
        string ObtenerHash(string token);
    }

    /// <summary>
    /// Genera refresh tokens opacos y los almacena hasheados (SHA-256).
    /// El token en claro solo se entrega una vez al cliente.
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        public string GenerarToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        public string ObtenerHash(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}
