using ServiciosGenerales.Aplicacion.Dtos.Admin;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface IObtenerHistorialTipoUseCase
    {
        Task<HistorialTipoResponseDto?> Ejecutar(int tipoUsuarioId, int anio, int mes);
    }

    /// <summary>
    /// Obtiene el detalle de ingresos y salidas de portería y parqueo de un tipo de
    /// persona concreto (Estudiante, Trabajador, Visitante) para un mes.
    /// Devuelve null si el tipo de persona no existe.
    /// </summary>
    public class ObtenerHistorialTipoUseCase : IObtenerHistorialTipoUseCase
    {
        private readonly ITiposUsuarioRepository _tiposUsuarioRepository;
        private readonly IRegistroPorteriaRepository _porteriaRepository;
        private readonly IRegistroParqueoRepository _parqueoRepository;

        public ObtenerHistorialTipoUseCase(
            ITiposUsuarioRepository tiposUsuarioRepository,
            IRegistroPorteriaRepository porteriaRepository,
            IRegistroParqueoRepository parqueoRepository)
        {
            _tiposUsuarioRepository = tiposUsuarioRepository;
            _porteriaRepository = porteriaRepository;
            _parqueoRepository = parqueoRepository;
        }

        public async Task<HistorialTipoResponseDto?> Ejecutar(int tipoUsuarioId, int anio, int mes)
        {
            var tipos = await _tiposUsuarioRepository.ObtenerTodosAsync();
            var tipo = tipos.FirstOrDefault(t => t.Id == tipoUsuarioId);
            if (tipo is null)
                return null;

            var registrosPorteria = await _porteriaRepository.ObtenerHistorialAsync();
            var porteria = registrosPorteria
                .Where(r => r.FechaEntrada.Year == anio && r.FechaEntrada.Month == mes &&
                            r.Usuario != null && r.Usuario.TipoUsuarioId == tipoUsuarioId)
                .Select(r => new HistorialPorteriaDto
                {
                    IdRegistro = r.Id,
                    NombreCompleto = r.Usuario?.NombreCompleto ?? "",
                    DocumentoIdentidad = r.Usuario?.DocumentoIdentidad ?? "",
                    FechaEntrada = r.FechaEntrada,
                    FechaSalida = r.FechaSalida,
                    PuertaEntrada = r.PuertaEntrada,
                    PuertaSalida = r.PuertaSalida,
                    MotivoVisita = r.MotivoVisita,
                    AreaDestino = r.AreaDestino,
                })
                .ToList();

            var registrosParqueo = await _parqueoRepository.ObtenerHistorialAsync();
            var parqueo = registrosParqueo
                .Where(r => r.FechaIngreso.Year == anio && r.FechaIngreso.Month == mes &&
                            r.Vehiculo?.Usuario != null && r.Vehiculo.Usuario.TipoUsuarioId == tipoUsuarioId)
                .Select(r => new HistorialParqueoDto
                {
                    IdRegistro = r.Id,
                    Matricula = r.Vehiculo?.Matricula ?? "",
                    Marca = r.Vehiculo?.Marca,
                    Modelo = r.Vehiculo?.Modelo,
                    Color = r.Vehiculo?.Color,
                    NombreCompleto = r.Vehiculo?.Usuario?.NombreCompleto ?? "",
                    DocumentoIdentidad = r.Vehiculo?.Usuario?.DocumentoIdentidad ?? "",
                    FechaIngreso = r.FechaIngreso,
                    FechaSalida = r.FechaSalida,
                    PuertaAcceso = r.PuertaAcceso,
                    Observaciones = r.Observaciones,
                })
                .ToList();

            return new HistorialTipoResponseDto
            {
                TipoUsuarioId = tipo.Id,
                TipoUsuario = tipo.Nombre,
                Anio = anio,
                Mes = mes,
                Porteria = porteria,
                Parqueo = parqueo,
            };
        }
    }
}