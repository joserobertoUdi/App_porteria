using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ServiciosGenerales.Aplicacion.Dtos.Auth;
using ServiciosGenerales.Aplicacion.Settings;

namespace ServiciosGenerales.Aplicacion.Services
{
    public interface ITokenService
    {
        string GenerarToken(LoginResponseDto usuario);
    }

    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(JwtSettings settings)
        {
            _settings = settings;
        }

        public string GenerarToken(LoginResponseDto usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.DocumentoIdentidad),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Role, usuario.RolNombre),
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
