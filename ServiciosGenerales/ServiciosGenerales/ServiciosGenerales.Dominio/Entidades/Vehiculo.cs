namespace ServiciosGenerales.Dominio.Entidades
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public required string Matricula { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Color { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public Usuario? Usuario { get; set; }
        public ICollection<RegistroParqueo> RegistrosParqueo { get; set; } = new List<RegistroParqueo>();
    }
}
