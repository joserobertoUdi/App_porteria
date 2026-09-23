namespace ServiciosGenerales.Dominio.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }
        public required string NombreCompleto { get; set; }
        public required string DocumentoIdentidad { get; set; }
        public int TipoUsuarioId { get; set; }
        public string? FotoUrl { get; set; }
        public string? PasswordHash { get; set; }
        public int? RolId { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Estado { get; set; } = true;
        public int IntentosFallidos { get; set; }
        public DateTime? BloqueoHasta { get; set; }

        public TiposUsuario? TipoUsuario { get; set; }
        public Rol? Rol { get; set; }
        public ICollection<RegistroPorteria> RegistrosPorteria { get; set; } = new List<RegistroPorteria>();
        public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    }
}
