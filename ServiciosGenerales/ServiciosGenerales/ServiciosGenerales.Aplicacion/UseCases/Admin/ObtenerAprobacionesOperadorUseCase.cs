using ServiciosGenerales.Aplicacion.Dtos.Admin;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface IObtenerAprobacionesOperadorUseCase
    {
        /// <summary>
        /// Obtiene todas las aprobaciones (portería + parqueo) de un operador.
        /// Devuelve null si el usuario no existe (el controlador responde 404).
        /// </summary>
        /// <param name="usuarioId">Id del guardia/portero.</param>
        /// <param name="desde">Inicio del rango (inclusivo), opcional.</param>
        /// <param name="hasta">Fin del rango (exclusivo), opcional.</param>
        Task<AprobacionesOperadorResponseDto?> Ejecutar(int usuarioId, DateTime? desde, DateTime? hasta);
    }

    /// <summary>
    /// Auditoría de guardia por día y turno: consolida los ingresos de portería y
    /// parqueo que un operador aprobó, con su estado (completada/pendiente).
    /// Los registros anteriores a esta funcionalidad quedan fuera porque no
    /// tienen operador asignado (RegistradoPorUsuarioId NULL).
    /// </summary>
    public class ObtenerAprobacionesOperadorUseCase : IObtenerAprobacionesOperadorUseCase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRegistroPorteriaRepository _porteriaRepository;
        private readonly IRegistroParqueoRepository _parqueoRepository;

        public ObtenerAprobacionesOperadorUseCase(
            IUsuarioRepository usuarioRepository,
            IRegistroPorteriaRepository porteriaRepository,
            IRegistroParqueoRepository parqueoRepository)
        {
            _usuarioRepository = usuarioRepository;
            _porteriaRepository = porteriaRepository;
            _parqueoRepository = parqueoRepository;
        }

        public async Task<AprobacionesOperadorResponseDto?> Ejecutar(int usuarioId, DateTime? desde, DateTime? hasta)
        {
            var operador = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
            if (operador is null)
                return null;

            var porteria = await _porteriaRepository.ObtenerPorRegistradoPorIdAsync(usuarioId);
            var parqueo = await _parqueoRepository.ObtenerPorRegistradoPorIdAsync(usuarioId);

            if (desde.HasValue || hasta.HasValue)
            {
                porteria = porteria.Where(r => EstaEnVentana(r.FechaEntrada, desde, hasta));
                parqueo = parqueo.Where(r => EstaEnVentana(r.FechaIngreso, desde, hasta));
            }

            var porteriaDto = porteria
                .Select(r => new AprobacionPorteriaDto
                {
                    IdRegistro = r.Id,
                    FechaEntrada = r.FechaEntrada,
                    FechaSalida = r.FechaSalida,
                    PuertaEntrada = r.PuertaEntrada,
                    PuertaSalida = r.PuertaSalida,
                    MotivoVisita = r.MotivoVisita,
                    AreaDestino = r.AreaDestino,
                    VisitanteNombre = r.Usuario?.NombreCompleto ?? "",
                    VisitanteDocumento = r.Usuario?.DocumentoIdentidad ?? "",
                })
                .ToList();

            var parqueoDto = parqueo
                .Select(r => new AprobacionParqueoDto
                {
                    IdRegistro = r.Id,
                    FechaIngreso = r.FechaIngreso,
                    FechaSalida = r.FechaSalida,
                    PuertaAcceso = r.PuertaAcceso,
                    Matricula = r.Vehiculo?.Matricula ?? "",
                    Marca = r.Vehiculo?.Marca,
                    Modelo = r.Vehiculo?.Modelo,
                    VisitanteNombre = r.Vehiculo?.Usuario?.NombreCompleto ?? "",
                    VisitanteDocumento = r.Vehiculo?.Usuario?.DocumentoIdentidad ?? "",
                })
                .ToList();

            var completadas = porteriaDto.Count(p => p.EstaCompletada)
                              + parqueoDto.Count(p => p.EstaCompletada);

            return new AprobacionesOperadorResponseDto
            {
                UsuarioId = operador.Id,
                NombreCompleto = operador.NombreCompleto,
                DocumentoIdentidad = operador.DocumentoIdentidad,
                Rol = operador.Rol?.Nombre ?? "",
                Desde = desde,
                Hasta = hasta,
                TotalCompletadas = completadas,
                TotalPendientes = porteriaDto.Count + parqueoDto.Count - completadas,
                Porteria = porteriaDto,
                Parqueo = parqueoDto,
            };
        }

        /// <summary>[desde, hasta) — hasta es exclusivo, en hora local del servidor.</summary>
        private static bool EstaEnVentana(DateTime fecha, DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && fecha < desde.Value)
                return false;
            if (hasta.HasValue && fecha >= hasta.Value)
                return false;
            return true;
        }
    }
}