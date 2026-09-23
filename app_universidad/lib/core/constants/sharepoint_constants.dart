/// Configuración del servicio central de gestión documental (SharepointApi).
///
/// Guarda las fotos de los registros de portería fuera de la base de datos: en
/// el registro solo queda una referencia `sharepoint:{uid}` de 45 caracteres.
///
/// `SHAREPOINT_BASE_URL` y `PROVIDER_KEY` se inyectan en tiempo de compilación
/// mediante `--dart-define`:
///
///     flutter run --dart-define=SHAREPOINT_BASE_URL=http://10.1.210.10/SharepointApi \
///                --dart-define=PROVIDER_KEY=tu_clave_aqui
class SharepointConstants {
  SharepointConstants._();

  /// URL base del servicio de gestión documental.
  ///
  /// Se puede sobrescribir sin tocar el código:
  ///
  ///     flutter run --dart-define=SHAREPOINT_BASE_URL=http://otro-servidor/api
  static const String baseUrl = String.fromEnvironment(
    'SHAREPOINT_BASE_URL',
    defaultValue: 'http://10.1.210.10/SharepointApi',
  );

  /// Cabecera de autenticación que espera el servicio.
  static const String providerKeyHeader = 'ProviderKey';

  /// Clave privada para autenticación con SharepointApi.
  ///
  /// Se debe inyectar en tiempo de compilación:
  ///
  ///     flutter run --dart-define=PROVIDER_KEY=tu_clave_aqui
  ///
  /// ⚠️  Nunca hardcodear esta clave en el código fuente.
  static const String providerKey = String.fromEnvironment(
    'PROVIDER_KEY',
    defaultValue: '',
  );

  /// Contenedor destino. Debe existir en la tabla `Contenedores` de la base de
  /// SharepointApi con `Activo = 1`; si no, el servicio responde 400.
  ///
  /// Sus columnas `ExtensionesPermitidas` y `TamanoMaximoMb` mandan sobre lo
  /// que se declara aquí abajo.
  static const String contenedor = 'parqueo';

  /// Identifica al sistema y al registro que originan el archivo, para la
  /// auditoría del servicio documental.
  static const String entidadOrigen = 'PorteriaEntrada';

  static const String uploadEndpoint = '/api/Archivos';
  static String contenidoEndpoint(String uid) => '/api/Archivos/$uid/contenido';

  /// URL absoluta para descargar la foto. Requiere la cabecera ProviderKey.
  static String contenidoUrl(String uid) => '$baseUrl${contenidoEndpoint(uid)}';

  static const Map<String, String> downloadHeaders = {
    providerKeyHeader: providerKey,
  };

  /// Prefijo de las referencias guardadas en `RegistrosPorteria.FotoUrl`.
  ///
  /// Se guarda `sharepoint:{uid}` en lugar de la URL completa para que un
  /// cambio de host o de ruta del servicio no invalide los registros ya
  /// existentes.
  static const String refPrefix = 'sharepoint:';

  static String buildRef(String uid) => '$refPrefix$uid';

  static bool isRef(String? valor) =>
      valor != null && valor.startsWith(refPrefix);

  static String? uidFromRef(String valor) =>
      isRef(valor) ? valor.substring(refPrefix.length) : null;

  /// Traduce una referencia guardada a una URL descargable.
  static String? resolveUrl(String? valor) {
    if (valor == null || valor.isEmpty) return null;
    final uid = uidFromRef(valor);
    return uid != null ? contenidoUrl(uid) : valor;
  }

  /// Tope global de la petición en SharepointApi
  /// (`Limites:TamanoMaximoPeticionMb`, por defecto 60 MB). El contenedor puede
  /// imponer uno menor mediante `TamanoMaximoMb`.
  static const int maxFileSizeBytes = 60 * 1024 * 1024;

  /// `true` si `PROVIDER_KEY` fue inyectada en tiempo de compilación.
  ///
  /// Si es `false`, las subidas de foto fallarán porque SharepointApi
  /// rechazará las peticiones sin cabecera `ProviderKey`.
  static bool get isConfigured => providerKey.isNotEmpty;
}
