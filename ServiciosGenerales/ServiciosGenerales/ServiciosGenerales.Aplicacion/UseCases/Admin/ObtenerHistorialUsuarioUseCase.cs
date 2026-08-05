using ServiciosGenerales.Aplicacion.Dtos.Admin;
using ServiciosGenerales.Dominio.Interfaces;

namespace ServiciosGenerales.Aplicacion.UseCases.Admin
{
    public interface IObtenerHistorialUsuarioUseCase
    {
        Task<HistorialUsuarioResponseDto?> Ejecutar(int id);
    }

    /// <summary>
    /// Obtiene el historial de visitas de un usuario (portería y parqueo).
    /// Devuelve null si el usuario no existe (el controlador responde 404).
    /// </summary>
    public class ObtenerHistorialUsuarioUseCase : IObtenerHistorialUsuarioUseCase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRegistroPorteriaRepository _porteriaRepository;
        private readonly IRegistroParqueoRepository _parqueoRepository;

        public ObtenerHistorialUsuarioUseCase(
            IUsuarioRepository usuarioRepository,
            IRegistroPorteriaRepository porteriaRepository,
            IRegistroParqueoRepository parqueoRepository)
        {
            _usuarioRepository = usuarioRepository;
            _porteriaRepository = porteriaRepository;
            _parqueoRepository = parqueoRepository;
        }

        public async Task<HistorialUsuarioResponseDto?> Ejecutar(int id)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
            if (usuario is null)
                return null;

            var porteria = await _porteriaRepository.ObtenerPorUsuarioIdAsync(id);
            var parqueo = await _parqueoRepository.ObtenerPorUsuarioIdAsync(id);

            return new HistorialUsuarioResponseDto
            {
                NombreCompleto = usuario.NombreCompleto,
                DocumentoIdentidad = usuario.DocumentoIdentidad,
                Porteria = porteria
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
                    .ToList(),
                Parqueo = parqueo
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
                    .ToList(),
            };
        }
    }
}
