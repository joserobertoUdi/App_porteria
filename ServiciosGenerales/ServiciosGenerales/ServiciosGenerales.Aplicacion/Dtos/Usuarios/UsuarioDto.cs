namespace ServiciosGenerales.Aplicacion.Dtos.Usuarios
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoIdentidad { get; set; } = string.Empty;
        public int TipoUsuarioId { get; set; }
        public string? FotoUrl { get; set; }
    }
}
