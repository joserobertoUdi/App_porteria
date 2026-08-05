using ServiciosGenerales.Aplicacion.Dtos.Admin;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface IObtenerEstadisticasTiposUseCase
    {
        Task<EstadisticasTiposResponseDto> Ejecutar(int anio, int mes);
    }

    /// <summary>
    /// Cuenta los ingresos y salidas de portería y parqueo agrupados por tipo de
    /// persona (Estudiante, Trabajador, Visitante) para un mes concreto.
    /// </summary>
    public class ObtenerEstadisticasTiposUseCase : IObtenerEstadisticasTiposUseCase
    {
        private readonly ITiposUsuarioRepository _tiposUsuarioRepository;
        private readonly IRegistroPorteriaRepository _porteriaRepository;
        private readonly IRegistroParqueoRepository _parqueoRepository;

        public ObtenerEstadisticasTiposUseCase(
            ITiposUsuarioRepository tiposUsuarioRepository,
            IRegistroPorteriaRepository porteriaRepository,
            IRegistroParqueoRepository parqueoRepository)
        {
            _tiposUsuarioRepository = tiposUsuarioRepository;
            _porteriaRepository = porteriaRepository;
            _parqueoRepository = parqueoRepository;
        }

        public async Task<EstadisticasTiposResponseDto> Ejecutar(int anio, int mes)
        {
            var tipos = await _tiposUsuarioRepository.ObtenerTodosAsync();
            var registrosPorteria = await _porteriaRepository.ObtenerHistorialAsync();
            var registrosParqueo = await _parqueoRepository.ObtenerHistorialAsync();

            var estadisticas = tipos.Select(tipo =>
            {
                var porteria = registrosPorteria
                    .Where(r => r.Usuario != null && r.Usuario.TipoUsuarioId == tipo.Id)
                    .ToList();
                var parqueo = registrosParqueo
                    .Where(r => r.Vehiculo?.Usuario != null && r.Vehiculo.Usuario.TipoUsuarioId == tipo.Id)
                    .ToList();

                return new EstadisticaTipoDto
                {
                    TipoUsuarioId = tipo.Id,
                    TipoUsuario = tipo.Nombre,
                    PorteriaIngresos = porteria.Count(r => r.FechaEntrada.Year == anio && r.FechaEntrada.Month == mes),
                    PorteriaSalidas = porteria.Count(r => r.FechaSalida?.Year == anio && r.FechaSalida?.Month == mes),
                    ParqueoIngresos = parqueo.Count(r => r.FechaIngreso.Year == anio && r.FechaIngreso.Month == mes),
                    ParqueoSalidas = parqueo.Count(r => r.FechaSalida?.Year == anio && r.FechaSalida?.Month == mes),
                };
            }).ToList();

            foreach (var t in estadisticas)
            {
                t.TotalIngresos = t.PorteriaIngresos + t.ParqueoIngresos;
                t.TotalSalidas = t.PorteriaSalidas + t.ParqueoSalidas;
            }

            return new EstadisticasTiposResponseDto { Anio = anio, Mes = mes, Tipos = estadisticas };
        }
    }
}