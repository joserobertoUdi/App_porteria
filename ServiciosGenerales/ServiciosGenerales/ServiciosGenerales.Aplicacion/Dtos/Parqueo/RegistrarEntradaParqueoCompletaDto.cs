using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Parqueo
{
    public class RegistrarEntradaParqueoCompletaDto
    {
        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [StringLength(20, MinimumLength = 3)]
        public required string DocumentoIdentidad { get; set; }

        [StringLength(200)]
        public string? NombreCompleto { get; set; }

        public string? TipoUsuarioId { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        [StringLength(20, MinimumLength = 3)]
        public required string Matricula { get; set; }

        [StringLength(50)]
        public string? Marca { get; set; }

        [StringLength(50)]
        public string? Modelo { get; set; }

        [StringLength(30)]
        public string? Color { get; set; }

        [Required(ErrorMessage = "La puerta de acceso es obligatoria.")]
        [StringLength(50)]
        public required string PuertaAcceso { get; set; }

        [StringLength(255)]
        public string? Observaciones { get; set; }
    }
}
