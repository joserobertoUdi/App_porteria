using ServiciosGenerales.Aplicacion.Dtos.Sistema;
using ServiciosGenerales.Aplicacion.Services;

namespace ServiciosGenerales.Aplicacion.UseCases.Sistema
{
    public interface IObtenerHoraServidorUseCase
    {
        HoraServidorDto Ejecutar();
    }

    /// <summary>
    /// Devuelve la hora actual del servidor (UTC y local), su offset UTC y la zona horaria.
    /// Es la fuente de verdad para que la app móvil unifique horas y valide la zona horaria.
    /// </summary>
    public class ObtenerHoraServidorUseCase : IObtenerHoraServidorUseCase
    {
        private readonly IServerClock _clock;

        public ObtenerHoraServidorUseCase(IServerClock clock)
        {
            _clock = clock;
        }

        public HoraServidorDto Ejecutar()
        {
            var ahoraUtc = _clock.UtcNow;
            var zona = TimeZoneInfo.Local;
            var local = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtc, zona);
            var offset = zona.GetUtcOffset(ahoraUtc);

            return new HoraServidorDto
            {
                FechaHoraUtc = ahoraUtc,
                FechaHoraLocal = local,
                OffsetUtc = OffsetUtcUtil.Formatear(offset),
                ZonaHoraria = zona.Id,
            };
        }
    }
}
