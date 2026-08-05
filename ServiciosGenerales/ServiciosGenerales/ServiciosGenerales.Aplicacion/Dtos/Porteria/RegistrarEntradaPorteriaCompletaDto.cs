using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Porteria
{
    public class RegistrarEntradaPorteriaCompletaDto
    {
        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [StringLength(20, MinimumLength = 3)]
        public required string DocumentoIdentidad { get; set; }

        [StringLength(200)]
        public string? NombreCompleto { get; set; }

        [Range(1, 3, ErrorMessage = "Tipo de usuario inválido.")]
        public int TipoUsuarioId { get; set; } = 3;

        [StringLength(50)]
        public string? PuertaEntrada { get; set; }

        [StringLength(500)]
        public string? MotivoVisita { get; set; }

        [StringLength(200)]
        public string? AreaDestino { get; set; }
    }
}
