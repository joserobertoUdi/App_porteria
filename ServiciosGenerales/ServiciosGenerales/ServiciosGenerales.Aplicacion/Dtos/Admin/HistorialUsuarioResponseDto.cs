namespace ServiciosGenerales.Aplicacion.Dtos.Admin
{
    public class HistorialUsuarioResponseDto
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoIdentidad { get; set; } = string.Empty;
        public List<HistorialPorteriaDto> Porteria { get; set; } = new();
        public List<HistorialParqueoDto> Parqueo { get; set; } = new();
    }
}
