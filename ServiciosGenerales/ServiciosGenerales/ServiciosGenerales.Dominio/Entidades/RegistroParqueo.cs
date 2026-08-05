namespace ServiciosGenerales.Dominio.Entidades
{
    public class RegistroParqueo
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        public DateTime? FechaSalida { get; set; }
        public required string PuertaAcceso { get; set; }
        public string? Observaciones { get; set; }

        public Vehiculo? Vehiculo { get; set; }
    }
}
