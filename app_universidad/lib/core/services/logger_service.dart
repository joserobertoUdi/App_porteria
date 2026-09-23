import 'package:flutter/foundation.dart';
import '../constants/error_messages.dart';

/// Servicio de logging centralizado para la aplicación.
///
/// En modo debug imprime a la consola. En release, solo registra internamente
/// (futuro: enviar a servicio de crash reporting como Crashlytics).
///
/// Uso:
/// ```dart
/// LoggerService.error('Error al cargar usuarios', e, stackTrace);
/// LoggerService.warning('Token próximo a expirar');
/// LoggerService.info('Sesión iniciada correctamente');
/// ```
class LoggerService {
  LoggerService._();

  // ── Niveles de log ────────────────────────────────────────────────────

  static void info(String message) {
    _log('INFO', message);
  }

  static void warning(String message) {
    _log('WARNING', message);
  }

  /// Registra un error con su stack trace.
  ///
  /// Si [error] es un [AppException], extrae el mensaje tipado.
  /// Si no, usa el mensaje genérico y registra el detalle original.
  static void error(
    String context,
    Object error, [
    StackTrace? stackTrace,
  ]) {
    final detail = error is AppException ? error.message : error.toString();
    _log('ERROR', '$context: $detail');
    if (stackTrace != null && kDebugMode) {
      debugPrintStack(stackTrace: stackTrace, label: 'Stack trace');
    }
    // Futuro: enviar a Crashlytics / backend de logging
    // Crashlytics.recordError(error, stackTrace, reason: context);
  }

  // ── Interno ──────────────────────────────────────────────────────────

  static void _log(String level, String message) {
    if (kDebugMode) {
      final timestamp = DateTime.now().toIso8601String().substring(0, 19);
      debugPrint('[$timestamp] $level: $message');
    }
    // En release, aquí se enviaría a un servicio de logging remoto.
  }
}
