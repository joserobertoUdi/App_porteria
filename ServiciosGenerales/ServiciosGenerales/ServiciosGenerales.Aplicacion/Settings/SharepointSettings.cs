namespace ServiciosGenerales.Aplicacion.Settings
{
    /// <summary>
    /// Configuración del servicio documental (SharepointApi).
    /// Se carga desde variables de entorno o appsettings.
    /// </summary>
    public class SharepointSettings
    {
        /// <summary>
        /// URL base del servicio (ej: http://10.1.210.10/SharepointApi).
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Clave de autenticación del provider.
        /// </summary>
        public string ProviderKey { get; set; } = string.Empty;

        /// <summary>
        /// Contenedor destino en el servicio documental.
        /// </summary>
        public string Contenedor { get; set; } = "parqueo";

        /// <summary>
        /// Entidad origen para auditoría.
        /// </summary>
        public string EntidadOrigen { get; set; } = "PorteriaEntrada";
    }
}
