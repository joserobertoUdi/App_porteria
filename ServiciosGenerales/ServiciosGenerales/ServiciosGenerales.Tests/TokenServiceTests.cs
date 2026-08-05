using System.IdentityModel.Tokens.Jwt;
using ServiciosGenerales.Aplicacion.Dtos.Auth;
using ServiciosGenerales.Aplicacion.Services;
using Xunit;

namespace ServiciosGenerales.Tests
{
    public class TokenServiceTests
    {
        [Fact]
        public void GenerarToken_ExpiraEn3HorasDesdeEmision()
        {
            var service = new TokenService(AuthFakes.JwtSettings);
            var dto = new LoginResponseDto
            {
                Token = "",
                RefreshToken = "RT-1",
                ExpiraEnMinutos = 180,
                NombreCompleto = "Usuario de prueba",
                DocumentoIdentidad = "12345678",
                TipoUsuarioId = 1,
                RolId = 1,
                RolNombre = "Administrador",
                FotoUrl = null,
            };

            var antes = DateTime.UtcNow;
            var jwt = service.GenerarToken(dto);
            var despues = DateTime.UtcNow;

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var rol = token.Claims.FirstOrDefault(
                c => c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role);
            Assert.NotNull(rol);
            Assert.Equal("Administrador", rol!.Value);

            var exp = token.ValidTo;
            Assert.True(exp >= antes.AddMinutes(179) && exp <= despues.AddMinutes(181));
            Assert.Equal("app-universidad", token.Audiences.First());
            Assert.Equal("UDI", token.Issuer);
        }
    }
}
