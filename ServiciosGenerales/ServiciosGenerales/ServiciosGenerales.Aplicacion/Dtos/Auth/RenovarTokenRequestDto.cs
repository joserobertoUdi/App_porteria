using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Auth
{
    public class RenovarTokenRequestDto
    {
        [Required(ErrorMessage = "Refresh token requerido.")]
        public required string RefreshToken { get; set; }
    }
}
