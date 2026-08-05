using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [StringLength(20, MinimumLength = 1)]
        public required string DocumentoIdentidad { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public required string Password { get; set; }
    }
}
