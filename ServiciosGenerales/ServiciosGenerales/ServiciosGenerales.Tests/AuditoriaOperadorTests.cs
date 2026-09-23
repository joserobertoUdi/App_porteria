using ServiciosGenerales.Aplicacion.Dtos.Parqueo;
using ServiciosGenerales.Aplicacion.Dtos.Porteria;
using ServiciosGenerales.Aplicacion.Dtos.Usuarios;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.UseCases.Admin;
using ServiciosGenerales.Aplicacion.UseCases.Parqueo;
using ServiciosGenerales.Aplicacion.UseCases.Porteria;
using ServiciosGenerales.Aplicacion.UseCases.Usuarios;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using Xunit;

namespace ServiciosGenerales.Tests
{
    /// <summary>
    /// Pruebas de la auditoría de guardia por día y turno: quién aprobó cada
    /// ingreso (operador) y el detalle/estado de las aprobaciones.
    /// </summary>
    public class AuditoriaOperadorTests
    {
        private static readonly DateTime Hoy = new(2026, 8, 28, 10, 0, 0, DateTimeKind.Local);

        // ── Fakes en memoria ────────────────────────────────────────────

        private sealed class MemoriaPorteriaRepository : IRegistroPorteriaRepository
        {
            public List<RegistroPorteria> Registros { get; } = new();

            public Task<int> RegistrarEntradaAsync(RegistroPorteria r) { Registros.Add(r); return Task.FromResult(r.Id); }
            public Task<bool> RegistrarSalidaAsync(int id, DateTime f, string? p) => Task.FromResult(true);
            public Task<IEnumerable<RegistroPorteria>> ObtenerVisitasActivasAsync() =>
                Task.FromResult<IEnumerable<RegistroPorteria>>(new List<RegistroPorteria>());
            public Task<IEnumerable<RegistroPorteria>> ObtenerHistorialAsync() =>
                Task.FromResult<IEnumerable<RegistroPorteria>>(Registros);
            public Task<IEnumerable<RegistroPorteria>> ObtenerPorUsuarioIdAsync(int uid) =>
                Task.FromResult<IEnumerable<RegistroPorteria>>(new List<RegistroPorteria>());
            public Task<IEnumerable<RegistroPorteria>> ObtenerPorRegistradoPorIdAsync(int registradoPor) =>
                Task.FromResult<IEnumerable<RegistroPorteria>>(Registros.Where(r => r.RegistradoPorUsuarioId == registradoPor).ToList());
        }

        private sealed class MemoriaParqueoRepository : IRegistroParqueoRepository
        {
            public List<RegistroParqueo> Registros { get; } = new();

            public Task<int> RegistrarEntradaAsync(RegistroParqueo r) { Registros.Add(r); return Task.FromResult(r.Id); }
            public Task<bool> RegistrarSalidaAsync(int id, DateTime f) => Task.FromResult(true);
            public Task<IEnumerable<RegistroParqueo>> ObtenerActivosAsync() =>
                Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());
            public Task<IEnumerable<RegistroParqueo>> ObtenerPorVehiculoIdAsync(int vid) =>
                Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());
            public Task<IEnumerable<RegistroParqueo>> ObtenerPorUsuarioIdAsync(int uid) =>
                Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());
            public Task<IEnumerable<RegistroParqueo>> ObtenerHistorialAsync() =>
                Task.FromResult<IEnumerable<RegistroParqueo>>(Registros);
            public Task<IEnumerable<RegistroParqueo>> ObtenerPorRegistradoPorIdAsync(int registradoPor) =>
                Task.FromResult<IEnumerable<RegistroParqueo>>(Registros.Where(r => r.RegistradoPorUsuarioId == registradoPor).ToList());
        }

        private sealed class FakeBuscarOCrearUsuario : IBuscarOCrearUsuarioPorCarnetUseCase
        {
            public Task<ResultadoBuscarOCrearUsuario> Ejecutar(string documento, string? nombre, int tipoUsuarioId)
                => Task.FromResult(new ResultadoBuscarOCrearUsuario
                {
                    Usuario = new Usuario
                    {
                        Id = 100,
                        NombreCompleto = string.IsNullOrWhiteSpace(nombre) ? documento : nombre,
                        DocumentoIdentidad = documento,
                        TipoUsuarioId = tipoUsuarioId,
                    },
                    Creado = false,
                });
        }

        private sealed class FakeSharepointService : ISharepointService
        {
            public Task<ArchivoSubidoDto?> SubirArchivoAsync(Stream stream, string nombre, string origen, string usuarioRegistro)
                => Task.FromResult<ArchivoSubidoDto?>(null);
        }

        // ── ObtenerAprobacionesOperadorUseCase ──────────────────────────

        [Fact]
        public async Task Aprobaciones_UsuarioInexistente_DevuelveNull()
        {
            var useCase = new ObtenerAprobacionesOperadorUseCase(
                new FakeUsuarioRepository(),
                new MemoriaPorteriaRepository(),
                new MemoriaParqueoRepository());

            var resultado = await useCase.Ejecutar(99, null, null);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task Aprobaciones_ConsolidaPorteriaYParqueo_ConEstados()
        {
            var operador = new Usuario { Id = 7, NombreCompleto = "Guardia Uno", DocumentoIdentidad = "OP-1", TipoUsuarioId = 3, Rol = new Rol { Id = 3, Nombre = "PorteroPorteria" } };
            var visitante = new Usuario { Id = 5, NombreCompleto = "Ana Visitante", DocumentoIdentidad = "1005", TipoUsuarioId = 3 };
            var vehiculo = new Vehiculo { Id = 1, UsuarioId = 5, Matricula = "ABC-777", Usuario = visitante };

            var porteriaRepo = new MemoriaPorteriaRepository();
            porteriaRepo.Registros.AddRange(new[]
            {
                new RegistroPorteria { Id = 1, UsuarioId = 5, Usuario = visitante, RegistradoPorUsuarioId = 7, FechaEntrada = Hoy, FechaSalida = Hoy.AddHours(2) },
                new RegistroPorteria { Id = 2, UsuarioId = 5, Usuario = visitante, RegistradoPorUsuarioId = 7, FechaEntrada = Hoy },
            });

            var parqueoRepo = new MemoriaParqueoRepository();
            parqueoRepo.Registros.AddRange(new[]
            {
                new RegistroParqueo { Id = 1, VehiculoId = 1, Vehiculo = vehiculo, RegistradoPorUsuarioId = 7, PuertaAcceso = "Principal", FechaIngreso = Hoy, FechaSalida = Hoy.AddHours(4) },
                new RegistroParqueo { Id = 2, VehiculoId = 1, Vehiculo = vehiculo, RegistradoPorUsuarioId = 7, PuertaAcceso = "Principal", FechaIngreso = Hoy },
            });

            var useCase = new ObtenerAprobacionesOperadorUseCase(new FakeUsuarioRepository(operador), porteriaRepo, parqueoRepo);

            var resultado = await useCase.Ejecutar(7, null, null);

            Assert.NotNull(resultado);
            Assert.Equal("Guardia Uno", resultado!.NombreCompleto);
            Assert.Equal("PorteroPorteria", resultado.Rol);
            Assert.Equal(2, resultado.Porteria.Count);
            Assert.Equal(2, resultado.Parqueo.Count);
            Assert.Equal(2, resultado.TotalCompletadas);
            Assert.Equal(2, resultado.TotalPendientes);

            var completada = resultado.Porteria.Single(p => p.IdRegistro == 1);
            Assert.True(completada.EstaCompletada);
            Assert.Equal("Ana Visitante", completada.VisitanteNombre);

            var pendiente = resultado.Parqueo.Single(p => p.IdRegistro == 2);
            Assert.False(pendiente.EstaCompletada);
            Assert.Equal("ABC-777", pendiente.Matricula);
        }

        [Fact]
        public async Task Aprobaciones_FiltraPorDia_VentanaInclusivaExclusiva()
        {
            var operador = new Usuario { Id = 7, NombreCompleto = "Guardia Uno", DocumentoIdentidad = "OP-1", TipoUsuarioId = 3 };
            var visitante = new Usuario { Id = 5, NombreCompleto = "Ana Visitante", DocumentoIdentidad = "1005", TipoUsuarioId = 3 };

            var porteriaRepo = new MemoriaPorteriaRepository();
            porteriaRepo.Registros.AddRange(new[]
            {
                new RegistroPorteria { Id = 1, UsuarioId = 5, Usuario = visitante, RegistradoPorUsuarioId = 7, FechaEntrada = new DateTime(2026, 8, 27, 23, 0, 0, DateTimeKind.Local) },
                new RegistroPorteria { Id = 2, UsuarioId = 5, Usuario = visitante, RegistradoPorUsuarioId = 7, FechaEntrada = new DateTime(2026, 8, 28, 8, 0, 0, DateTimeKind.Local) },
                new RegistroPorteria { Id = 3, UsuarioId = 5, Usuario = visitante, RegistradoPorUsuarioId = 7, FechaEntrada = new DateTime(2026, 8, 28, 23, 59, 0, DateTimeKind.Local) },
                new RegistroPorteria { Id = 4, UsuarioId = 5, Usuario = visitante, RegistradoPorUsuarioId = 7, FechaEntrada = new DateTime(2026, 8, 29, 0, 0, 0, DateTimeKind.Local) },
            });

            var useCase = new ObtenerAprobacionesOperadorUseCase(
                new FakeUsuarioRepository(operador),
                porteriaRepo,
                new MemoriaParqueoRepository());

            var desde = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Local);
            var hasta = new DateTime(2026, 8, 29, 0, 0, 0, DateTimeKind.Local);
            var resultado = await useCase.Ejecutar(7, desde, hasta);

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado!.Porteria.Count);
            Assert.Equal(new[] { 2, 3 }, resultado.Porteria.Select(p => p.IdRegistro).ToArray());
        }

        [Fact]
        public async Task Aprobaciones_SinRegistros_DevuelveCeros()
        {
            var operador = new Usuario { Id = 7, NombreCompleto = "Guardia Uno", DocumentoIdentidad = "OP-1", TipoUsuarioId = 3 };

            var useCase = new ObtenerAprobacionesOperadorUseCase(
                new FakeUsuarioRepository(operador),
                new MemoriaPorteriaRepository(),
                new MemoriaParqueoRepository());

            var resultado = await useCase.Ejecutar(7, null, null);

            Assert.NotNull(resultado);
            Assert.Empty(resultado!.Porteria);
            Assert.Empty(resultado.Parqueo);
            Assert.Equal(0, resultado.TotalCompletadas);
            Assert.Equal(0, resultado.TotalPendientes);
        }

        // ── Asignación de operador al registrar una entrada ─────────────

        [Fact]
        public async Task RegistrarEntradaPorteria_AsignaOperador()
        {
            var repo = new FakeRegistroPorteriaRepository();
            var useCase = new RegistrarEntradaUseCase(repo, new FakeServerClock(Hoy));

            await useCase.Ejecutar(new RegistrarEntradaDto
            {
                UsuarioId = 42,
                RegistradoPorUsuarioId = 7,
                PuertaEntrada = "Principal",
            });

            Assert.NotNull(repo.RegistroEntradaCapturado);
            Assert.Equal(7, repo.RegistroEntradaCapturado!.RegistradoPorUsuarioId);
        }

        [Fact]
        public async Task RegistrarEntradaParqueo_AsignaOperador()
        {
            var repo = new FakeRegistroParqueoRepository();
            var useCase = new RegistrarEntradaParqueoUseCase(repo, new FakeServerClock(Hoy));

            await useCase.Ejecutar(new RegistrarEntradaParqueoDto
            {
                VehiculoId = 9,
                RegistradoPorUsuarioId = 7,
                PuertaAcceso = "Principal",
            });

            Assert.NotNull(repo.RegistroEntradaCapturado);
            Assert.Equal(7, repo.RegistroEntradaCapturado!.RegistradoPorUsuarioId);
        }

        [Fact]
        public async Task RegistrarEntradaPorteriaCompleta_PasaOperadorAlRegistro()
        {
            var repo = new FakeRegistroPorteriaRepository();
            var registrarEntrada = new RegistrarEntradaUseCase(repo, new FakeServerClock(Hoy));
            var useCase = new RegistrarEntradaPorteriaCompletaUseCase(
                new FakeBuscarOCrearUsuario(),
                registrarEntrada,
                new FakeSharepointService());

            await useCase.Ejecutar(new RegistrarEntradaPorteriaCompletaDto
            {
                DocumentoIdentidad = "1005",
                NombreCompleto = "Ana Visitante",
                TipoUsuarioId = 3,
                PuertaEntrada = "Principal",
            }, 7);

            Assert.NotNull(repo.RegistroEntradaCapturado);
            Assert.Equal(7, repo.RegistroEntradaCapturado!.RegistradoPorUsuarioId);
            Assert.Equal(100, repo.RegistroEntradaCapturado!.UsuarioId);
        }
    }
}