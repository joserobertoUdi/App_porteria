using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Porteria
{
    public class RegistrarEntradaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "UsuarioId inválido.")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Usuario que aprueba el ingreso (portero/guardia). Lo resuelve el
        /// servidor desde el JWT; la app no lo envía.
        /// </summary>
        public int? RegistradoPorUsuarioId { get; set; }

        [StringLength(50)]
        public string? PuertaEntrada { get; set; }

        [StringLength(500)]
        public string? MotivoVisita { get; set; }

        [StringLength(200)]
        public string? AreaDestino { get; set; }

        /// <summary>Referencia <c>sharepoint:{uid}</c> a la foto del ingreso.</summary>
        [StringLength(200)]
        public string? FotoUrl { get; set; }
    }
}
