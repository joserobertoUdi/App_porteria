using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Dominio.Entidades;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Tests
{
    /// <summary>Reloj del servidor con hora fija para pruebas.</summary>
    public sealed class FakeServerClock : IServerClock
    {
        public DateTime Fijo { get; }

        public FakeServerClock(DateTime fijo)
        {
            Fijo = DateTime.SpecifyKind(fijo, DateTimeKind.Local);
        }

        public DateTime Now => Fijo;
        public DateTime UtcNow => Fijo.ToUniversalTime();
    }

    /// <summary>Fake de IRegistroPorteriaRepository que captura lo que se persiste.</summary>
    public sealed class FakeRegistroPorteriaRepository : IRegistroPorteriaRepository
    {
        public RegistroPorteria? RegistroEntradaCapturado { get; private set; }
        public DateTime? FechaSalidaCapturada { get; private set; }
        public int IdSalidaCapturado { get; private set; }
        public string? PuertaSalidaCapturada { get; private set; }

        public Task<int> RegistrarEntradaAsync(RegistroPorteria registro)
        {
            RegistroEntradaCapturado = registro;
            return Task.FromResult(1);
        }

        public Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida, string? puertaSalida)
        {
            IdSalidaCapturado = id;
            FechaSalidaCapturada = fechaSalida;
            PuertaSalidaCapturada = puertaSalida;
            return Task.FromResult(true);
        }

        public Task<IEnumerable<RegistroPorteria>> ObtenerVisitasActivasAsync() =>
            Task.FromResult<IEnumerable<RegistroPorteria>>(new List<RegistroPorteria>());

        public Task<IEnumerable<RegistroPorteria>> ObtenerHistorialAsync() =>
            Task.FromResult<IEnumerable<RegistroPorteria>>(new List<RegistroPorteria>());

        public Task<IEnumerable<RegistroPorteria>> ObtenerPorUsuarioIdAsync(int usuarioId) =>
            Task.FromResult<IEnumerable<RegistroPorteria>>(new List<RegistroPorteria>());

        public Task<IEnumerable<RegistroPorteria>> ObtenerPorRegistradoPorIdAsync(int registradoPorUsuarioId) =>
            Task.FromResult<IEnumerable<RegistroPorteria>>(new List<RegistroPorteria>());
    }

    /// <summary>Fake de IRegistroParqueoRepository que captura lo que se persiste.</summary>
    public sealed class FakeRegistroParqueoRepository : IRegistroParqueoRepository
    {
        public RegistroParqueo? RegistroEntradaCapturado { get; private set; }
        public DateTime? FechaSalidaCapturada { get; private set; }

        public Task<int> RegistrarEntradaAsync(RegistroParqueo registro)
        {
            RegistroEntradaCapturado = registro;
            return Task.FromResult(1);
        }

        public Task<bool> RegistrarSalidaAsync(int id, DateTime fechaSalida)
        {
            FechaSalidaCapturada = fechaSalida;
            return Task.FromResult(true);
        }

        public Task<IEnumerable<RegistroParqueo>> ObtenerActivosAsync() =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());

        public Task<IEnumerable<RegistroParqueo>> ObtenerPorVehiculoIdAsync(int vehiculoId) =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());

        public Task<IEnumerable<RegistroParqueo>> ObtenerPorUsuarioIdAsync(int usuarioId) =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());

        public Task<IEnumerable<RegistroParqueo>> ObtenerPorRegistradoPorIdAsync(int registradoPorUsuarioId) =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());

        public Task<IEnumerable<RegistroParqueo>> ObtenerHistorialAsync() =>
            Task.FromResult<IEnumerable<RegistroParqueo>>(new List<RegistroParqueo>());
    }
}
