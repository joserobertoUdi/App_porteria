namespace ServiciosGenerales.Aplicacion.Dtos.Auth
{
    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiraEnMinutos { get; set; }
        public required string NombreCompleto { get; set; }
        public required string DocumentoIdentidad { get; set; }
        public int TipoUsuarioId { get; set; }
        public int? RolId { get; set; }
        public required string RolNombre { get; set; }
        public string? FotoUrl { get; set; }
    }
}
