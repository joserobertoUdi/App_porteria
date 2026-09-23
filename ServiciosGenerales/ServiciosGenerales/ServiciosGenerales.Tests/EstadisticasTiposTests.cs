using ServiciosGenerales.Aplicacion.UseCases.Admin;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;
using Xunit;

namespace ServiciosGenerales.Tests
{
    public sealed class FakeTiposUsuarioRepository : ITiposUsuarioRepository
    {
        public List<TiposUsuario> Tipos { get; set; } = new();

        public Task<IEnumerable<TiposUsuario>> ObtenerTodosAsync() =>
            Task.FromResult<IEnumerable<TiposUsuario>>(Tipos);
    }

    public sealed class FakePorteriaHistorialRepository : IRegistroPorteriaRepository
    {
        public List<RegistroPorteria> Historial { get; set; } = new();

        public Task<IEnumerable<RegistroPorteria>> ObtenerHistorialAsync() =>
            Task.FromResult<IEnumerable<RegistroPorteria>>(Historial);

        public Task<int> RegistrarEntradaAsync(RegistroPorteria registro) => Task.FromResult(1);
        public Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida, string? puertaSalida) => Task.FromResult(true);
        public Task<IEnumerable<RegistroPorteria>> ObtenerVisitasActivasAsync() =>
            Task.FromResult<IEnumerable<RegistroPorteria>>(new List<RegistroPorteria>());
        public Task<IEnumerable<RegistroPorteria>> ObtenerPorUsuarioIdAsync(int usuarioId) =>
            Task.FromResult<IEnumerable<RegistroPorteria>>(new List<RegistroPorteria>());
        public Task<IEnumerable<RegistroPorteria>> ObtenerPorRegistradoPorIdAsync(int registradoPorUsuarioId) =>
            Task.FromResult<IEnumerable<RegistroPorteria>>(Historial);
    }

    public sealed class FakeParqueoHistorialRepository : IRegistroParqueoRepository
    {
        public List<RegistroParqueo> Historial { get; set; } = new();

        public Task<IEnumerable<RegistroParqueo>> ObtenerHistorialAsync() =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(Historial);

        public Task<int> RegistrarEntradaAsync(RegistroParqueo registro) => Task.FromResult(1);
        public Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida) => Task.FromResult(true);
        public Task<IEnumerable<RegistroParqueo>> ObtenerActivosAsync() =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());
        public Task<IEnumerable<RegistroParqueo>> ObtenerPorVehiculoIdAsync(int vehiculoId) =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());
        public Task<IEnumerable<RegistroParqueo>> ObtenerPorUsuarioIdAsync(int usuarioId) =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());
        public Task<IEnumerable<RegistroParqueo>> ObtenerPorRegistradoPorIdAsync(int registradoPorUsuarioId) =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(Historial);
    }

    /// <summary>
    /// Pruebas de las estadísticas mensuales y el historial por tipo de persona.
    /// </summary>
    public class EstadisticasTiposTests
    {
        private static readonly DateTime Julio2026 = new(2026, 7, 15, 9, 0, 0, DateTimeKind.Local);

        private static (FakeTiposUsuarioRepository Tipos, FakePorteriaHistorialRepository Porteria,
            FakeParqueoHistorialRepository Parqueo) CrearMundo()
        {
            var tipos = new FakeTiposUsuarioRepository
            {
                Tipos = new List<TiposUsuario>
                {
                    new() { Id = 1, Nombre = "Estudiante" },
                    new() { Id = 2, Nombre = "Trabajador" },
                    new() { Id = 3, Nombre = "Visitante" },
                }
            };

            var estudiante = new Usuario { Id = 10, NombreCompleto = "Ana Estudiante", DocumentoIdentidad = "10001", TipoUsuarioId = 1 };
            var trabajador = new Usuario { Id = 20, NombreCompleto = "Luis Trabajador", DocumentoIdentidad = "10002", TipoUsuarioId = 2 };
            var vehiculo = new Vehiculo { Id = 5, UsuarioId = 20, Matricula = "ABC-1289", Marca = "Honda", Modelo = "2016", Color = "Rojo", Usuario = trabajador };

            var porteria = new FakePorteriaHistorialRepository
            {
                Historial = new List<RegistroPorteria>
                {
                    new() { Id = 1, UsuarioId = 10, Usuario = estudiante, FechaEntrada = Julio2026, FechaSalida = Julio2026.AddHours(2) },
                    new() { Id = 2, UsuarioId = 20, Usuario = trabajador, FechaEntrada = Julio2026.AddDays(1), FechaSalida = Julio2026.AddDays(1).AddHours(3) },
                    new() { Id = 3, UsuarioId = 20, Usuario = trabajador, FechaEntrada = new DateTime(2026, 6, 5, 8, 0, 0, DateTimeKind.Local) },
                    new() { Id = 4, UsuarioId = 10, Usuario = estudiante, FechaEntrada = new DateTime(2026, 8, 2, 8, 0, 0, DateTimeKind.Local) },
                }
            };

            var parqueo = new FakeParqueoHistorialRepository
            {
                Historial = new List<RegistroParqueo>
                {
                    new() { Id = 1, VehiculoId = 5, Vehiculo = vehiculo, PuertaAcceso = "Principal", FechaIngreso = Julio2026.AddDays(2), FechaSalida = Julio2026.AddDays(2).AddHours(4) },
                    new() { Id = 2, VehiculoId = 5, Vehiculo = vehiculo, PuertaAcceso = "Principal", FechaIngreso = new DateTime(2026, 6, 10, 8, 0, 0, DateTimeKind.Local) },
                }
            };

            return (tipos, porteria, parqueo);
        }

        [Fact]
        public async Task EstadisticasTipos_CuentaIngresosYSalidasDelMesPorTipo()
        {
            var (tipos, porteria, parqueo) = CrearMundo();
            var useCase = new ObtenerEstadisticasTiposUseCase(tipos, porteria, parqueo);

            var resultado = await useCase.Ejecutar(2026, 7);

            Assert.Equal(2026, resultado.Anio);
            Assert.Equal(7, resultado.Mes);
            Assert.Equal(3, resultado.Tipos.Count);

            var estudiante = resultado.Tipos.Single(t => t.TipoUsuarioId == 1);
            Assert.Equal(1, estudiante.PorteriaIngresos);
            Assert.Equal(1, estudiante.PorteriaSalidas);
            Assert.Equal(0, estudiante.ParqueoIngresos);
            Assert.Equal(0, estudiante.ParqueoSalidas);

            var trabajador = resultado.Tipos.Single(t => t.TipoUsuarioId == 2);
            Assert.Equal(1, trabajador.PorteriaIngresos);
            Assert.Equal(1, trabajador.PorteriaSalidas);
            Assert.Equal(1, trabajador.ParqueoIngresos);
            Assert.Equal(1, trabajador.ParqueoSalidas);
        }

        [Fact]
        public async Task EstadisticasTipos_ExcluyeRegistrosDeOtrosMeses()
        {
            var (tipos, porteria, parqueo) = CrearMundo();
            var useCase = new ObtenerEstadisticasTiposUseCase(tipos, porteria, parqueo);

            var junio = await useCase.Ejecutar(2026, 6);
            var agosto = await useCase.Ejecutar(2026, 8);

            // En junio solo existen el registro de portería y el de parqueo del Trabajador.
            Assert.Equal(0, junio.Tipos.Single(t => t.TipoUsuarioId == 1).PorteriaIngresos);
            Assert.Equal(1, junio.Tipos.Single(t => t.TipoUsuarioId == 2).PorteriaIngresos);
            Assert.Equal(1, junio.Tipos.Single(t => t.TipoUsuarioId == 2).ParqueoIngresos);

            // En agosto solo existe una entrada de portería del Estudiante.
            Assert.Equal(1, agosto.Tipos.Single(t => t.TipoUsuarioId == 1).PorteriaIngresos);
            Assert.Equal(0, agosto.Tipos.Single(t => t.TipoUsuarioId == 2).ParqueoIngresos);
        }

        [Fact]
        public async Task EstadisticasTipos_CalculaTotalesPorTipo()
        {
            var (tipos, porteria, parqueo) = CrearMundo();
            var useCase = new ObtenerEstadisticasTiposUseCase(tipos, porteria, parqueo);

            var resultado = await useCase.Ejecutar(2026, 7);

            var trabajador = resultado.Tipos.Single(t => t.TipoUsuarioId == 2);
            Assert.Equal(2, trabajador.TotalIngresos);   // 1 portería + 1 parqueo
            Assert.Equal(2, trabajador.TotalSalidas);    // 1 portería + 1 parqueo

            var visitante = resultado.Tipos.Single(t => t.TipoUsuarioId == 3);
            Assert.Equal(0, visitante.TotalIngresos);
            Assert.Equal(0, visitante.TotalSalidas);
        }

        [Fact]
        public async Task HistorialTipo_TipoInexistente_DevuelveNull()
        {
            var (tipos, porteria, parqueo) = CrearMundo();
            var useCase = new ObtenerHistorialTipoUseCase(tipos, porteria, parqueo);

            var resultado = await useCase.Ejecutar(99, 2026, 7);

            Assert.Null(resultado);
        }

        [Fact]
        public async Task HistorialTipo_DevuelveSoloRegistrosDelTipoYMes()
        {
            var (tipos, porteria, parqueo) = CrearMundo();
            var useCase = new ObtenerHistorialTipoUseCase(tipos, porteria, parqueo);

            var resultado = await useCase.Ejecutar(2, 2026, 7);

            Assert.NotNull(resultado);
            Assert.Equal("Trabajador", resultado!.TipoUsuario);
            Assert.Single(resultado.Porteria);
            Assert.Single(resultado.Parqueo);

            var parqueoRegistro = resultado.Parqueo.Single();
            Assert.Equal("ABC-1289", parqueoRegistro.Matricula);
            Assert.Equal("Luis Trabajador", parqueoRegistro.NombreCompleto);
        }

        [Fact]
        public async Task HistorialTipo_ExcluyeRegistrosDeOtroTipo()
        {
            var (tipos, porteria, parqueo) = CrearMundo();
            var useCase = new ObtenerHistorialTipoUseCase(tipos, porteria, parqueo);

            var resultado = await useCase.Ejecutar(1, 2026, 7);

            Assert.NotNull(resultado);
            Assert.Equal("Estudiante", resultado!.TipoUsuario);
            Assert.Single(resultado.Porteria);   // solo la entrada de Ana en julio
            Assert.Empty(resultado.Parqueo);     // los vehículos son del Trabajador
        }
    }
}