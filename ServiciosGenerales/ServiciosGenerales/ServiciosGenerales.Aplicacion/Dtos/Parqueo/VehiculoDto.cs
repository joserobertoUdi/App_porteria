namespace ServiciosGenerales.Aplicacion.Dtos.Parqueo
{
    public class VehiculoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Color { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
