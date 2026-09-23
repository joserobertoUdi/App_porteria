import 'package:local_auth/local_auth.dart';

/// Servicio delgado que encapsula `local_auth` para autenticación biométrica.
///
/// Responsabilidades:
/// 1. Verificar si el dispositivo tiene hardware biométrico y si está habilitado.
/// 2. Ejecutar la autenticación (huella / Face ID / PIN del dispositivo).
///
/// No gestiona preferencias del usuario; eso corresponde a [UserPrefs].
class BiometricService {
  BiometricService({LocalAuthentication? localAuth})
      : _localAuth = localAuth ?? LocalAuthentication();

  final LocalAuthentication _localAuth;

  /// Indica si el dispositivo soporta biometría (huella, Face ID, etc.)
  /// y si el al menos un método está registrado y habilitado.
  ///
  /// **No** indica si el usuario de la app activó la opción; para eso
  /// consultar [UserPrefs.isBiometricEnabled].
  Future<bool> isDeviceSupported() async {
    try {
      final canAuthenticate = await _localAuth.canCheckBiometrics;
      final isDeviceSupported = await _localAuth.isDeviceSupported();
      return canAuthenticate && isDeviceSupported;
    } catch (_) {
      // En emuladores o dispositivos sin hardware, puede lanzar excepción.
      return false;
    }
  }

  /// Devuelve la lista de biometrías disponibles en el dispositivo
  /// (ej. `[BiometricType.fingerprint]`, `[BiometricType.face]`).
  Future<List<BiometricType>> availableBiometrics() async {
    try {
      return await _localAuth.getAvailableBiometrics();
    } catch (_) {
      return [];
    }
  }

  /// Lanza el prompt nativo de autenticación biométrica.
  ///
  /// Devuelve `true` si el usuario autenticó con éxito, `false` si canceló
  /// o falló. El [message] se muestra en el diálogo nativo.
  Future<bool> authenticate({
    String message = 'Verifique su identidad',
  }) async {
    try {
      return await _localAuth.authenticate(
        localizedReason: message,
        options: const AuthenticationOptions(
          stickyAuth: true,
          biometricOnly: false,
        ),
      );
    } catch (_) {
      return false;
    }
  }
}
