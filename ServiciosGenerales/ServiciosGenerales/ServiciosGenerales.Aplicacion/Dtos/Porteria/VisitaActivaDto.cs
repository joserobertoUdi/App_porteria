namespace ServiciosGenerales.Aplicacion.Dtos.Porteria
{
    public class VisitaActivaDto
    {
        public int IdRegistro { get; set; }
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoIdentidad { get; set; } = string.Empty;
        public DateTime FechaEntrada { get; set; }
        public string? MotivoVisita { get; set; }
        public string? PuertaEntrada { get; set; }
    }
}
