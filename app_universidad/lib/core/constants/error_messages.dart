/// Catálogo centralizado de mensajes de error para la aplicación.
///
/// Todos los mensajes están en español comprensible para el usuario final.
/// Se clasifican por categoría para facilitar el mantenimiento.
///
/// Uso:
/// ```dart
/// throw AppException.network('No se pudo conectar al servidor');
/// // o
/// ScaffoldMessenger.of(context).showSnackBar(
///   SnackBar(content: Text(AppErrors.network.generic)),
/// );
/// ```
class AppErrors {
  AppErrors._();

  // ── Red / Conectividad ────────────────────────────────────────────────

  static const network = NetworkErrors._();

  // ── Autenticación ─────────────────────────────────────────────────────

  static const auth = AuthErrors._();

  // ── Validación de datos ───────────────────────────────────────────────

  static const validation = ValidationErrors._();

  // ── Servidor ──────────────────────────────────────────────────────────

  static const server = ServerErrors._();

  // ── Biométrico ────────────────────────────────────────────────────────

  static const biometric = BiometricErrors._();

  // ── General ───────────────────────────────────────────────────────────

  static const generic = 'Ocurrió un error inesperado. Intente de nuevo.';
}

// ── Clases de errores tipados ─────────────────────────────────────────────

class NetworkErrors {
  const NetworkErrors._();

  String get generic =>
      'No se pudo conectar con el servidor. Verifique su conexión a la red.';
  String get timeout =>
      'La conexión tardó demasiado. Intente de nuevo.';
  String get refused =>
      'El servidor rechazó la conexión. Verifique la configuración de red.';
  String get dns =>
      'No se pudo encontrar el servidor. Verifique la dirección.';
  String get ssl =>
      'Error de certificado SSL. La conexión no es segura.';
}

class AuthErrors {
  const AuthErrors._();

  String get invalidCredentials => 'Usuario o contraseña incorrectos.';
  String get accountLocked =>
      'La cuenta está bloqueada por demasiados intentos fallidos. '
      'Espere 15 minutos e intente de nuevo.';
  String get sessionExpired =>
      'Su sesión expiró. Inicie sesión nuevamente.';
  String get tokenRefreshFailed =>
      'No se pudo renovar la sesión. Inicie sesión nuevamente.';
  String get unauthorized =>
      'No tiene permiso para realizar esta acción.';
  String get timezoneMismatch =>
      'La zona horaria del dispositivo no coincide con la del servidor. '
      'Ajuste la hora del dispositivo e intente de nuevo.';
  String get biometricNotConfigured =>
      'El login biométrico no está configurado. Inicie sesión con su contraseña.';
}

class ValidationErrors {
  const ValidationErrors._();

  String get requiredField => 'Este campo es obligatorio.';
  String get invalidDni => 'Ingrese un número de documento válido.';
  String get passwordTooShort => 'La contraseña debe tener al menos 6 caracteres.';
  String get invalidPlate => 'Ingrese una placa válida.';
  String get fieldsRequired => 'Complete todos los campos obligatorios.';
}

class ServerErrors {
  const ServerErrors._();

  String get generic => 'Error del servidor. Intente de nuevo en unos momentos.';
  String get notFound => 'El recurso solicitado no fue encontrado.';
  String get conflict =>
      'Conflicto con los datos existentes. Verifique la información.';
  String get tooManyRequests =>
      'Demasiadas solicitudes. Espere un momento e intente de nuevo.';
  String get internal =>
      'Error interno del servidor. Contacte al administrador.';
  String fromStatus(int status) {
    return switch (status) {
      400 => 'Solicitud incorrecta. Verifique los datos ingresados.',
      401 => 'No autorizado. Inicie sesión nuevamente.',
      403 => 'Acceso denegado. No tiene permisos.',
      404 => 'Recurso no encontrado.',
      409 => 'Conflicto con datos existentes.',
      429 => 'Demasiadas solicitudes. Espere un momento.',
      500 => 'Error interno del servidor.',
      502 => 'El servidor no está disponible. Intente más tarde.',
      503 => 'Servicio temporalmente no disponible.',
      _ => 'Error del servidor ($status).',
    };
  }
}

class BiometricErrors {
  const BiometricErrors._();

  String get notAvailable =>
      'Su dispositivo no soporta autenticación biométrica.';
  String get notEnrolled =>
      'No hay biometría configurada en el dispositivo. '
      'Agregue una huella o Face ID en la configuración del sistema.';
  String get cancelled =>
      'La autenticación biométrica fue cancelada.';
  String get failed =>
      'La autenticación biométrica falló. Intente de nuevo.';
  String get lockedOut =>
      'Demasiados intentos fallidos. Use su contraseña o PIN del dispositivo.';
}

// ── Excepción tipada de la aplicación ──────────────────────────────────────

/// Excepción base para errores de la aplicación con mensaje de usuario.
///
/// Las subclases permiten distinguir el tipo de error sin depender del
/// contenido del mensaje.
class AppException implements Exception {
  final String message;
  const AppException(this.message);

  @override
  String toString() => message;
}

class NetworkException extends AppException {
  const NetworkException(super.message);
}

class AuthException extends AppException {
  const AuthException(super.message);
}

class ValidationException extends AppException {
  const ValidationException(super.message);
}

class ServerException extends AppException {
  final int? statusCode;
  const ServerException(super.message, {this.statusCode});
}

// ── Utilidades ─────────────────────────────────────────────────────────────

/// Extrae el mensaje de usuario desde una excepción.
///
/// Si [error] es una [AppException] (tipada por el sistema), usa su mensaje
/// directo. Para cualquier otra excepción se devuelve el mensaje genérico.
String mensajeDeError(Object error) {
  if (error is AppException) return error.message;
  return AppErrors.generic;
}
