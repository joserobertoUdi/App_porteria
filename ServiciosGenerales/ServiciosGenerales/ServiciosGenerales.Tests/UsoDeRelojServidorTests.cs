using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.UseCases.Porteria;
using ServiciosGenerales.Aplicacion.UseCases.Parqueo;
using ServiciosGenerales.Aplicacion.UseCases.Sistema;
using Xunit;

namespace ServiciosGenerales.Tests
{
    /// <summary>
    /// Pruebas de la unificación de horas: todo registro de negocio usa la hora
    /// del servidor (IServerClock) y el cliente no puede influir.
    /// </summary>
    public class UsoDeRelojServidorTests
    {
        private static readonly DateTime HoraFija = new(2026, 7, 31, 10, 30, 0, DateTimeKind.Local);

        [Fact]
        public async Task RegistrarEntradaPorteria_UsaHoraDelServidor()
        {
            var clock = new FakeServerClock(HoraFija);
            var repo = new FakeRegistroPorteriaRepository();
            var useCase = new RegistrarEntradaUseCase(repo, clock);

            await useCase.Ejecutar(new RegistrarEntradaDto { UsuarioId = 1, PuertaEntrada = "Principal" });

            Assert.Equal(HoraFija, repo.RegistroEntradaCapturado!.FechaEntrada);
        }

        [Fact]
        public async Task RegistrarSalidaPorteria_IgnoraFechaDelCliente_YUsaHoraDelServidor()
        {
            var clock = new FakeServerClock(HoraFija);
            var repo = new FakeRegistroPorteriaRepository();
            var useCase = new RegistrarSalidaUseCase(repo, clock);

            // El DTO ya no expone FechaSalida: el cliente no tiene forma de influir.
            await useCase.Ejecutar(new RegistrarSalidaDto { Id = 5, PuertaSalida = "Principal" });

            Assert.Equal(5, repo.IdSalidaCapturado);
            Assert.Equal(HoraFija, repo.FechaSalidaCapturada);
            Assert.Equal("Principal", repo.PuertaSalidaCapturada);
        }

        [Fact]
        public async Task RegistrarEntradaParqueo_UsaHoraDelServidor()
        {
            var clock = new FakeServerClock(HoraFija);
            var repo = new FakeRegistroParqueoRepository();
            var useCase = new RegistrarEntradaParqueoUseCase(repo, clock);

            await useCase.Ejecutar(new RegistrarEntradaParqueoDto { VehiculoId = 3, PuertaAcceso = "Este" });

            Assert.Equal(HoraFija, repo.RegistroEntradaCapturado!.FechaIngreso);
        }

        [Fact]
        public async Task RegistrarSalidaParqueo_UsaHoraDelServidor()
        {
            var clock = new FakeServerClock(HoraFija);
            var repo = new FakeRegistroParqueoRepository();
            var useCase = new RegistrarSalidaParqueoUseCase(repo, clock);

            await useCase.Ejecutar(new RegistrarSalidaParqueoDto { Id = 7 });

            Assert.Equal(HoraFija, repo.FechaSalidaCapturada);
        }
    }

    public class ObtenerHoraServidorUseCaseTests
    {
        [Fact]
        public void Ejecutar_DevuelveHoraUtcYLocalDelServidor()
        {
            var ahoraUtc = DateTime.UtcNow;
            var zona = TimeZoneInfo.Local;
            var local = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtc, zona);
            var clock = new FakeServerClock(local);
            var useCase = new ObtenerHoraServidorUseCase(clock);

            var resultado = useCase.Ejecutar();

            var offsetEsperado = zona.GetUtcOffset(ahoraUtc);

            Assert.Equal(Truncar(ahoraUtc), Truncar(resultado.FechaHoraUtc));
            Assert.Equal(Truncar(local), Truncar(resultado.FechaHoraLocal));
            Assert.Equal(OffsetUtcUtil.Formatear(offsetEsperado), resultado.OffsetUtc);
            Assert.Equal(zona.Id, resultado.ZonaHoraria);
        }

        private static DateTime Truncar(DateTime d) =>
            new(d.Ticks - d.Ticks % TimeSpan.TicksPerSecond);
    }
}
