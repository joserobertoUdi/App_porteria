namespace ServiciosGenerales.Aplicacion.Settings
{
    public class JwtSettings
    {
        public required string Key { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public int ExpireMinutes { get; set; } = 180;
        public int RefreshTokenExpireDays { get; set; } = 7;
    }
}
