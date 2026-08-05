using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Parqueo
{
    public class RegistrarEntradaParqueoDto
    {
        public int VehiculoId { get; set; }

        [Required(ErrorMessage = "La puerta de acceso es obligatoria.")]
        [StringLength(50)]
        public required string PuertaAcceso { get; set; }

        [StringLength(255)]
        public string? Observaciones { get; set; }
    }
}
