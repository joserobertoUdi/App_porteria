namespace ServiciosGenerales.Aplicacion.Dtos.Auth
{
    public class LoginBloqueadoResponseDto
    {
        public string Mensaje { get; set; } = string.Empty;
        public DateTime BloqueadoHasta { get; set; }
    }
}
