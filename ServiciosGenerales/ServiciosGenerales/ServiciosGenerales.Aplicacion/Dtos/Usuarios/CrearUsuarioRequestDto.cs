using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Usuarios
{
    public class CrearUsuarioRequestDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(200, MinimumLength = 2)]
        public required string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [StringLength(20, MinimumLength = 3)]
        public required string DocumentoIdentidad { get; set; }

        [Range(1, 3, ErrorMessage = "Tipo de usuario inválido.")]
        public int TipoUsuarioId { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }

        [Range(1, 3, ErrorMessage = "Rol de sistema inválido. Debe ser Administrador (1), Portero Parqueo (2) o Portero Portería (3).")]
        public int RolId { get; set; }
    }
}
