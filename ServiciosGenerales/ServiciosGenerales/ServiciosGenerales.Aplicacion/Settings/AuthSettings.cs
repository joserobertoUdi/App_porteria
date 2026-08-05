namespace ServiciosGenerales.Aplicacion.Settings
{
    public class AuthSettings
    {
        public int MaxIntentosFallidos { get; set; } = 5;
        public int DuracionBloqueoMinutos { get; set; } = 15;
    }
}
