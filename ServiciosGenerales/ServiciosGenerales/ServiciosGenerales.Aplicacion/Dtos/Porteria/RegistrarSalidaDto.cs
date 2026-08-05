using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Porteria
{
    public class RegistrarSalidaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Id de registro inválido.")]
        public int Id { get; set; }

        [StringLength(50)]
        public string? PuertaSalida { get; set; }
    }
}
