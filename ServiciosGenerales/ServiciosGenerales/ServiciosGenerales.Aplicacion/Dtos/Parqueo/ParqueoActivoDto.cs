namespace ServiciosGenerales.Aplicacion.Dtos.Parqueo
{
    public class ParqueoActivoDto
    {
        public int IdRegistro { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Color { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoIdentidad { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string PuertaAcceso { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }
}
