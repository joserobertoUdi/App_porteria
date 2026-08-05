namespace ServiciosGenerales.Dominio.Entidades
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public required string TokenHash { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaRevocacion { get; set; }
        public string? ReemplazadoPor { get; set; }

        public bool EsValido => FechaRevocacion == null && FechaExpiracion > DateTime.UtcNow;

        public Usuario? Usuario { get; set; }
    }
}
