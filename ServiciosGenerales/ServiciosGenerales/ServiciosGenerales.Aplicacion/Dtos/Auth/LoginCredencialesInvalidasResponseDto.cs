namespace ServiciosGenerales.Aplicacion.Dtos.Auth
{
    public class LoginCredencialesInvalidasResponseDto
    {
        public string Mensaje { get; set; } = string.Empty;
        public int IntentosRestantes { get; set; }
    }
}
