using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiciosGenerales.Aplicacion.Services;
using ServiciosGenerales.Aplicacion.Settings;

namespace ServiciosGenerales.Infraestructura.Services
{
    /// <summary>
    /// Cliente del servicio documental (SharepointApi).
    /// Sube archivos vía multipart/form-data y devuelve la referencia sharepoint:{uid}.
    /// </summary>
    public class SharepointService : ISharepointService
    {
        private readonly HttpClient _httpClient;
        private readonly SharepointSettings _settings;
        private readonly ILogger<SharepointService> _logger;

        public SharepointService(
            HttpClient httpClient,
            IOptions<SharepointSettings> settings,
            ILogger<SharepointService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ArchivoSubidoDto?> SubirArchivoAsync(
            Stream stream,
            string nombreArchivo,
            string referenciaOrigen,
            string usuarioRegistro)
        {
            if (stream == null || stream.Length == 0)
            {
                _logger.LogWarning("SubirArchivoAsync: stream vacío para ref={Referencia}", referenciaOrigen);
                return null;
            }

            try
            {
                var uri = new Uri($"{_settings.BaseUrl}/api/Archivos");

                using var contenido = new StreamContent(stream);
                contenido.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                using var peticion = new MultipartFormDataContent();
                peticion.Add(contenido, "Archivo", nombreArchivo);
                peticion.Add(new StringContent(_settings.Contenedor), "Contenedor");
                peticion.Add(new StringContent(_settings.EntidadOrigen), "EntidadOrigen");
                peticion.Add(new StringContent(referenciaOrigen), "ReferenciaOrigen");
                peticion.Add(new StringContent(usuarioRegistro), "UsuarioRegistro");

                // SharepointApi espera la clave en un header propio.
                peticion.Headers.Add("ProviderKey", _settings.ProviderKey);

                _logger.LogInformation(
                    "Subiendo foto a SharepointApi: archivo={Archivo}, ref={Referencia}",
                    nombreArchivo, referenciaOrigen);

                var respuesta = await _httpClient.PostAsync(uri, peticion);
                respuesta.EnsureSuccessStatusCode();

                var json = await respuesta.Content.ReadAsStringAsync();
                var resultado = System.Text.Json.JsonSerializer.Deserialize<SharepointResponse>(json);

                var uid = resultado?.Datos?.Uid;
                if (string.IsNullOrEmpty(uid))
                {
                    _logger.LogWarning("SharepointApi no devolvió uid. Respuesta: {Respuesta}", json);
                    return null;
                }

                return new ArchivoSubidoDto
                {
                    Uid = uid,
                    Referencia = $"sharepoint:{uid}",
                    NombreArchivo = resultado?.Datos?.NombreArchivo ?? nombreArchivo,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir foto a SharepointApi");
                return null;
            }
        }

        // Modelos internos para deserializar la respuesta de SharepointApi.
        private class SharepointResponse
        {
            public SharepointDatos? Datos { get; set; }
        }

        private class SharepointDatos
        {
            public string? Uid { get; set; }
            public string? NombreArchivo { get; set; }
        }
    }
}
