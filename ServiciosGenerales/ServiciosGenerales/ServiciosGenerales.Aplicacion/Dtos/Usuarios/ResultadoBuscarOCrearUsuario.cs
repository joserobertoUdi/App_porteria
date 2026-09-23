using ServiciosGenerales.Dominio.Entidades;

namespace ServiciosGenerales.Aplicacion.Dtos.Usuarios
{
    public class ResultadoBuscarOCrearUsuario
    {
        public required Usuario Usuario { get; set; }
        public bool Creado { get; set; }
    }
}
