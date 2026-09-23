using System.ComponentModel.DataAnnotations;

namespace ServiciosGenerales.Aplicacion.Dtos.Usuarios
{
    public class ActualizarUsuarioRequestDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(200, MinimumLength = 2)]
        public required string NombreCompleto { get; set; }

        [Range(1, 3, ErrorMessage = "Tipo de usuario inválido.")]
        public int TipoUsuarioId { get; set; }

        [Range(1, 3, ErrorMessage = "Rol de sistema inválido. Debe ser Administrador (1), Portero Parqueo (2) o Portero Portería (3).")]
        public int RolId { get; set; }

        public bool Estado { get; set; } = true;
    }
}
