namespace ServiciosGenerales.Aplicacion.Dtos.Auth
{
    public class LoginResultDto
    {
        public bool Exito { get; set; }
        public LoginResponseDto? Usuario { get; set; }
        public bool CuentaBloqueada { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
        public int IntentosRestantes { get; set; }
    }
}
