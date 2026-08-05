using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Porteria
{
    public class RegistrarEntradaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "UsuarioId inválido.")]
        public int UsuarioId { get; set; }

        [StringLength(50)]
        public string? PuertaEntrada { get; set; }

        [StringLength(500)]
        public string? MotivoVisita { get; set; }

        [StringLength(200)]
        public string? AreaDestino { get; set; }
    }
}
