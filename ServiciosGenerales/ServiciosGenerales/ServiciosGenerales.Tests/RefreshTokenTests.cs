using ServiciosGenerales.Aplicacion.Dtos.Auth;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.Settings;
using ServiciosGenerales.Aplicacion.UseCases.Auth;
using ServiciosGenerales.Dominio.Entidades;
using Xunit;

namespace ServiciosGenerales.Tests
{
    public class RefreshTokenServiceTests
    {
        [Fact]
        public void GenerarToken_DevuelveValorNoVacioYDiferenteCadaVez()
        {
            var service = new RefreshTokenService();

            var t1 = service.GenerarToken();
            var t2 = service.GenerarToken();

            Assert.False(string.IsNullOrWhiteSpace(t1));
            Assert.NotEqual(t1, t2);
        }

        [Fact]
        public void ObtenerHash_EsDeterministaYDe64Hex()
        {
            var service = new RefreshTokenService();

            var h1 = service.ObtenerHash("mi-token");
            var h2 = service.ObtenerHash("mi-token");
            var h3 = service.ObtenerHash("otro-token");

            Assert.Equal(h1, h2);
            Assert.Equal(64, h1.Length);
            Assert.NotEqual(h1, h3);
        }
    }

    public class LoginUseCaseTests
    {
        private static readonly DateTime FechaFija = new(2026, 7, 31, 10, 0, 0, DateTimeKind.Utc);

        private static (LoginUseCase useCase, FakeUsuarioRepository usuarios, FakeRefreshTokenRepository refresh)
            CrearContexto(Usuario? usuario)
        {
            var usuarios = new FakeUsuarioRepository(usuario ?? AuthFakes.CrearUsuarioActivo());
            var refresh = new FakeRefreshTokenRepository();
            var factory = new RefreshTokenFactory(
                refresh,
                new FakeRefreshTokenService(),
                new FakeServerClock(FechaFija),
                AuthFakes.JwtSettings);

            var useCase = new LoginUseCase(
                usuarios,
                factory,
                new FakeServerClock(FechaFija),
                AuthFakes.JwtSettings,
                AuthFakes.AuthSettings);
            return (useCase, usuarios, refresh);
        }

        [Fact]
        public async Task Login_ConCredencialesValidas_GeneraYPersisteRefreshToken()
        {
            var (useCase, _, refresh) = CrearContexto(null);
            var request = new LoginRequestDto { DocumentoIdentidad = "12345678", Password = "clave123" };

            var resultado = await useCase.Ejecutar(request);

            Assert.True(resultado.Exito);
            Assert.NotNull(resultado.Usuario);
            Assert.NotNull(resultado.Usuario!.RefreshToken);
            Assert.Equal(180, resultado.Usuario.ExpiraEnMinutos);
            Assert.Single(refresh.Tokens);
            Assert.Equal("HASH-RT-1", refresh.Tokens[0].TokenHash);
            Assert.Equal(DateTime.SpecifyKind(FechaFija, DateTimeKind.Local).ToUniversalTime().AddDays(7),
                refresh.Tokens[0].FechaExpiracion);
        }

        [Fact]
        public async Task Login_ContrasenaIncorrecta_NoBloquea_InformaIntentosRestantes()
        {
            var (useCase, _, refresh) = CrearContexto(null);
            var request = new LoginRequestDto { DocumentoIdentidad = "12345678", Password = "incorrecta" };

            var resultado = await useCase.Ejecutar(request);

            Assert.False(resultado.Exito);
            Assert.False(resultado.CuentaBloqueada);
            Assert.Equal(4, resultado.IntentosRestantes);
            Assert.Empty(refresh.Tokens);
        }

        [Fact]
        public async Task Login_Tras5Fallidos_BloqueaCuentaYRechazaContrasenaCorrecta()
        {
            var (useCase, usuarios, _) = CrearContexto(null);
            var fallido = new LoginRequestDto { DocumentoIdentidad = "12345678", Password = "incorrecta" };
            var correcto = new LoginRequestDto { DocumentoIdentidad = "12345678", Password = "clave123" };

            for (var i = 0; i < 4; i++)
                await useCase.Ejecutar(fallido);

            var quinto = await useCase.Ejecutar(fallido);
            Assert.False(quinto.Exito);
            Assert.True(quinto.CuentaBloqueada);
            Assert.NotNull(quinto.BloqueadoHasta);
            Assert.True(quinto.BloqueadoHasta > FechaFija);

            // Con la contraseña correcta, pero cuenta bloqueada, se rechaza.
            var trasBloqueo = await useCase.Ejecutar(correcto);
            Assert.False(trasBloqueo.Exito);
            Assert.True(trasBloqueo.CuentaBloqueada);

            // El usuario quedó bloqueado hasta Now + 15 min.
            var usuario = usuarios._ObtenerTodos().First();
            Assert.NotNull(usuario.BloqueoHasta);
            Assert.Equal(0, usuario.IntentosFallidos);
        }

        [Fact]
        public async Task Login_Correcto_DespuesDeFallos_ReseteaContador()
        {
            var (useCase, usuarios, _) = CrearContexto(null);
            var fallido = new LoginRequestDto { DocumentoIdentidad = "12345678", Password = "incorrecta" };
            var correcto = new LoginRequestDto { DocumentoIdentidad = "12345678", Password = "clave123" };

            await useCase.Ejecutar(fallido);
            await useCase.Ejecutar(fallido);

            var resultado = await useCase.Ejecutar(correcto);

            Assert.True(resultado.Exito);
            var usuario = usuarios._ObtenerTodos().First();
            Assert.Equal(0, usuario.IntentosFallidos);
            Assert.Null(usuario.BloqueoHasta);
        }

        [Fact]
        public async Task Login_UsuarioInactivo_DevuelveFalloSinBloqueo()
        {
            var (useCase, _, refresh) = CrearContexto(AuthFakes.CrearUsuarioActivo(estado: false));
            var request = new LoginRequestDto { DocumentoIdentidad = "12345678", Password = "clave123" };

            var resultado = await useCase.Ejecutar(request);

            Assert.False(resultado.Exito);
            Assert.False(resultado.CuentaBloqueada);
            Assert.Empty(refresh.Tokens);
        }
    }

    public class RenovarTokenUseCaseTests
    {
        private static readonly DateTime FechaFija = new(2026, 7, 31, 10, 0, 0, DateTimeKind.Utc);

        private static (RenovarTokenUseCase useCase, FakeRefreshTokenRepository refresh, FakeUsuarioRepository usuarios)
            CrearContexto(string? tokenEnClaro, Usuario? usuario = null, DateTime? expiracion = null, bool? revocado = null)
        {
            var usuarios = new FakeUsuarioRepository(usuario ?? AuthFakes.CrearUsuarioActivo());
            var refresh = new FakeRefreshTokenRepository();
            var service = new FakeRefreshTokenService();
            var factory = new RefreshTokenFactory(
                refresh,
                service,
                new FakeServerClock(FechaFija),
                AuthFakes.JwtSettings);

            if (!string.IsNullOrEmpty(tokenEnClaro))
            {
                // Consumimos un token emitido previamente para imitar el flujo real.
                service.GenerarToken();
                var entidad = new RefreshToken
                {
                    Id = 1,
                    UsuarioId = 1,
                    TokenHash = service.ObtenerHash(tokenEnClaro),
                    FechaExpiracion = expiracion ?? DateTime.UtcNow.AddDays(7),
                    FechaRevocacion = revocado == true ? DateTime.UtcNow : null,
                };
                refresh.AgregarAsync(entidad).GetAwaiter().GetResult();
            }

            var useCase = new RenovarTokenUseCase(refresh, service, factory, usuarios, AuthFakes.JwtSettings);
            return (useCase, refresh, usuarios);
        }

        [Fact]
        public async Task Refresh_Valido_RotaTokenYEmitaUnoNuevo()
        {
            var (useCase, refresh, _) = CrearContexto("RT-valido");

            var resultado = await useCase.Ejecutar("RT-valido");

            Assert.NotNull(resultado);
            Assert.NotNull(resultado!.RefreshToken);
            Assert.Equal("RT-2", resultado.RefreshToken);
            Assert.Equal(2, refresh.Tokens.Count);

            var anterior = refresh.Tokens[0];
            Assert.NotNull(anterior.FechaRevocacion);
            Assert.Equal("HASH-RT-2", anterior.ReemplazadoPor);

            var nuevo = refresh.Tokens[1];
            Assert.Null(nuevo.FechaRevocacion);
            Assert.Equal("HASH-RT-2", nuevo.TokenHash);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Refresh_ConTokenVacio_DevuelveNull(string? token)
        {
            var (useCase, refresh, _) = CrearContexto(null);

            var resultado = await useCase.Ejecutar(token!);

            Assert.Null(resultado);
            Assert.Empty(refresh.Tokens);
        }

        [Fact]
        public async Task Refresh_TokenDesconocido_DevuelveNull()
        {
            var (useCase, refresh, _) = CrearContexto("RT-existente");

            var resultado = await useCase.Ejecutar("RT-otro");

            Assert.Null(resultado);
            Assert.Single(refresh.Tokens);
        }

        [Fact]
        public async Task Refresh_TokenExpirado_DevuelveNull()
        {
            var (useCase, _, _) = CrearContexto("RT-expirado", expiracion: DateTime.UtcNow.AddDays(-1));

            var resultado = await useCase.Ejecutar("RT-expirado");

            Assert.Null(resultado);
        }

        [Fact]
        public async Task Refresh_TokenRevocado_DevuelveNull()
        {
            var (useCase, _, _) = CrearContexto("RT-revocado", revocado: true);

            var resultado = await useCase.Ejecutar("RT-revocado");

            Assert.Null(resultado);
        }

        [Fact]
        public async Task Refresh_UsuarioInactivo_DevuelveNull()
        {
            var (useCase, refresh, _) = CrearContexto("RT-inactivo", usuario: AuthFakes.CrearUsuarioActivo(estado: false));

            var resultado = await useCase.Ejecutar("RT-inactivo");

            Assert.Null(resultado);
        }
    }
}
