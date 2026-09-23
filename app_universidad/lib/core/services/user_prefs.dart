import 'package:shared_preferences/shared_preferences.dart';

/// Persistencia local de preferencias del usuario.
///
/// Usa `shared_preferences` para guardar datos small que sobreviven reinicios:
/// - DNI del último usuario que inició sesión (para pre-llenar el campo).
/// - Preferencia de login biométrico (si el usuario activó huella/Face ID).
class UserPrefs {
  UserPrefs._();

  static const String _keyLastDni = 'last_dni';
  static const String _keyBiometricEnabled = 'biometric_enabled';
  static const String _keyLastPassword = 'last_password';

  // ── Último DNI ──────────────────────────────────────────────────────────

  /// Guarda el DNI del usuario que acaba de iniciar sesión.
  static Future<void> saveLastDni(String dni) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyLastDni, dni);
  }

  /// Recupera el DNI del último usuario que inició sesión.
  /// Devuelve `null` si no hay ninguno guardado.
  static Future<String?> getLastDni() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyLastDni);
  }

  /// Elimina el DNI guardado (útil al cerrar sesión si se desea).
  static Future<void> clearLastDni() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keyLastDni);
  }

  // ── Contraseña cacheada (para login biométrico) ────────────────────────

  /// Guarda la contraseña del usuario (necesaria para login biométrico).
  /// Solo se almacena si el usuario habilita la biometría.
  static Future<void> savePassword(String password) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyLastPassword, password);
  }

  /// Recupera la contraseña cacheada. `null` si no hay ninguna guardada.
  static Future<String?> getPassword() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_keyLastPassword);
  }

  /// Elimina la contraseña cacheada.
  static Future<void> clearPassword() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keyLastPassword);
  }

  // ── Preferencia biométrica ─────────────────────────────────────────────

  /// Guarda si el usuario habilitó el login biométrico.
  static Future<void> setBiometricEnabled(bool enabled) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_keyBiometricEnabled, enabled);
  }

  /// Devuelve `true` si el usuario habilitó el login biométrico.
  /// `false` por defecto (nunca ha configurado).
  static Future<bool> isBiometricEnabled() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getBool(_keyBiometricEnabled) ?? false;
  }

  /// Elimina la preferencia biométrica.
  static Future<void> clearBiometricEnabled() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keyBiometricEnabled);
  }
}
