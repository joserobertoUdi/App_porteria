namespace ServiciosGenerales.Dominio.Entidades
{
    public class RegistroPorteria
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaEntrada { get; set; } = DateTime.Now;
        public DateTime? FechaSalida { get; set; }
        public string? PuertaEntrada { get; set; }
        public string? PuertaSalida { get; set; }
        public string? MotivoVisita { get; set; }
        public string? AreaDestino { get; set; }

        public Usuario? Usuario { get; set; }
    }
}
