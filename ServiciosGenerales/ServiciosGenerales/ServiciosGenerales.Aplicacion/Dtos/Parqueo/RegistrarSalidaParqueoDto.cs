using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Parqueo
{
    public class RegistrarSalidaParqueoDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Id de registro inválido.")]
        public int Id { get; set; }
    }
}
