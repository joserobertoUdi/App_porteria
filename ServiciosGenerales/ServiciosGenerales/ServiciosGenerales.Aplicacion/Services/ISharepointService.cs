namespace ServiciosGenerales.Aplicacion.Services
{
    /// <summary>
    /// Resultado de subir un archivo al servicio documental (SharepointApi).
    /// </summary>
    public class ArchivoSubidoDto
    {
        public string Uid { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interfaz para subir archivos al servicio documental central.
    /// </summary>
    public interface ISharepointService
    {
        /// <summary>
        /// Sube un archivo (foto) al servicio documental y devuelve la referencia.
        /// </summary>
        /// <param name="stream">Contenido del archivo.</param>
        /// <param name="nombreArchivo">Nombre original del archivo.</param>
        /// <param name="referenciaOrigen">Identificador del registro dueño (ej: C.I.).</param>
        /// <param name="usuarioRegistro">Nombre del usuario que registra.</param>
        /// <returns>Referencia <c>sharepoint:{uid}</c> o null si falló.</returns>
        Task<ArchivoSubidoDto?> SubirArchivoAsync(
            Stream stream,
            string nombreArchivo,
            string referenciaOrigen,
            string usuarioRegistro);
    }
}
