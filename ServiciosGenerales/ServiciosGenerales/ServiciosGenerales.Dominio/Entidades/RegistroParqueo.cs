namespace ServiciosGenerales.Dominio.Entidades
{
    public class RegistroParqueo
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }

        /// <summary>
        /// Usuario (portero/guardia) que aprobó el ingreso.
        /// Nullable porque los registros previos a esta funcionalidad no lo tienen.
        /// </summary>
        public int? RegistradoPorUsuarioId { get; set; }

        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        public DateTime? FechaSalida { get; set; }
        public required string PuertaAcceso { get; set; }
        public string? Observaciones { get; set; }

        public Vehiculo? Vehiculo { get; set; }
        public Usuario? RegistradoPor { get; set; }
    }
}
