namespace ServiciosGenerales.Aplicacion.Dtos.Sistema
{
    public class HoraServidorDto
    {
        public DateTime FechaHoraUtc { get; set; }
        public DateTime FechaHoraLocal { get; set; }
        public required string OffsetUtc { get; set; }
        public required string ZonaHoraria { get; set; }
    }
}
