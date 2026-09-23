namespace ServiciosGenerales.Dominio.Entidades
{
    public class RegistroPorteria
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        /// <summary>
        /// Usuario (portero/guardia) que aprobó el ingreso.
        /// Nullable porque los registros previos a esta funcionalidad no lo tienen.
        /// </summary>
        public int? RegistradoPorUsuarioId { get; set; }

        public DateTime FechaEntrada { get; set; } = DateTime.Now;
        public DateTime? FechaSalida { get; set; }
        public string? PuertaEntrada { get; set; }
        public string? PuertaSalida { get; set; }
        public string? MotivoVisita { get; set; }
        public string? AreaDestino { get; set; }

        /// <summary>
        /// Referencia a la foto del ingreso en el servicio documental, con el
        /// formato <c>sharepoint:{uid}</c>.
        /// <para>
        /// No se guarda la imagen ni una URL absoluta: solo el identificador.
        /// Así, si cambia el host o la ruta del servicio, los registros
        /// históricos siguen resolviendo.
        /// </para>
        /// </summary>
        public string? FotoUrl { get; set; }

        public Usuario? Usuario { get; set; }
        public Usuario? RegistradoPor { get; set; }
    }
}
