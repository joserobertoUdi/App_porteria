import 'dart:async';
import 'package:flutter/material.dart';
import '../../core/network/api_client.dart';
import '../../core/services/biometric_service.dart';
import '../../core/services/user_prefs.dart';
import '../../core/utils/zona_horaria_util.dart';
import '../../data/datasources/auth_remote_data_source.dart';
import '../../data/datasources/sistema_remote_data_source.dart';
import '../../data/models/hora_servidor_model.dart';
import '../../data/models/login_response_model.dart';

/// Estado de autenticación de la aplicación.
///
/// Gestiona:
/// - Login / logout con JWT + refresh token.
/// - Persistencia del último DNI ingresado (para pre-llenar el campo).
/// - Login biométrico (huella / Face ID) cuando el usuario lo activa.
class AuthProvider extends ChangeNotifier {
  final ApiClient _apiClient;
  final AuthRemoteDataSource _authDataSource;
  final SistemaRemoteDataSource _sistemaDataSource;
  final BiometricService _biometricService;
  final GlobalKey<NavigatorState>? _navigatorKey;

  LoginResponseModel? _user;
  bool _loading = false;
  String? _error;
  HoraServidorModel? _horaServidor;
  bool _refreshEnProceso = false;
  Timer? _sesionTimer;

  // ── Biometría ──────────────────────────────────────────────────────────
  bool _biometricAvailable = false;
  bool _biometricEnabledByUser = false;
  String? _lastDni;

  AuthProvider(
    this._apiClient,
    this._authDataSource,
    this._sistemaDataSource, {
    GlobalKey<NavigatorState>? navigatorKey,
    BiometricService? biometricService,
  })  : _navigatorKey = navigatorKey,
        _biometricService = biometricService ?? BiometricService() {
    _apiClient.onRefreshRequested = _renovarSesion;
    _apiClient.onSessionExpired = _manejarSesionExpirada;
    _init();
  }

  /// Carga asíncrona de preferencias al iniciar el provider.
  Future<void> _init() async {
    _lastDni = await UserPrefs.getLastDni();
    _biometricEnabledByUser = await UserPrefs.isBiometricEnabled();
    _biometricAvailable = await _biometricService.isDeviceSupported();
    notifyListeners();
  }

  // ── Getters públicos ──────────────────────────────────────────────────

  LoginResponseModel? get user => _user;
  bool get isLoggedIn => _user != null;
  bool get loading => _loading;
  String? get error => _error;
  String get rolNombre => _user?.rolNombre ?? '';
  String get token => _user?.token ?? '';
  String get nombreCompleto => _user?.nombreCompleto ?? '';
  HoraServidorModel? get horaServidor => _horaServidor;

  bool get esAdministrador => rolNombre == 'Administrador';
  bool get esPorteroParqueo => rolNombre == 'PorteroParqueo';
  bool get esPorteroPorteria => rolNombre == 'PorteroPorteria';

  /// DNI del último usuario que inició sesión (para pre-llenar el campo).
  String? get lastDni => _lastDni;

  /// `true` si el dispositivo tiene hardware biométrico disponible.
  bool get biometricAvailable => _biometricAvailable;

  /// `true` si el usuario activó la opción de login biométrico.
  bool get biometricEnabled => _biometricEnabledByUser;

  /// `true` si se puede mostrar el ícono de huella:
  /// hardware disponible Y usuario la activó.
  bool get canUseBiometric => _biometricAvailable && _biometricEnabledByUser;

  // ── Login con contraseña ──────────────────────────────────────────────

  Future<bool> login(String documentoIdentidad, String password) async {
    _loading = true;
    _error = null;
    notifyListeners();

    try {
      // La app solo funciona si el dispositivo está en la misma zona horaria
      // que el servidor: se unifica la hora con el reloj del servidor.
      final hora = await _sistemaDataSource.obtenerHoraServidor();
      if (!ZonaHorariaUtil.esMismaZona(
          DateTime.now().timeZoneOffset, hora.offsetUtc)) {
        _error = 'Zona horaria del dispositivo no coincide con la del servidor '
            '(${hora.zonaHoraria}, ${hora.offsetUtc}). Ajuste la hora del '
            'dispositivo y reintente.';
        _loading = false;
        notifyListeners();
        return false;
      }
      _horaServidor = hora;

      final response = await _authDataSource.login(documentoIdentidad, password);
      _aplicarSesion(response);

      // Persistir DNI y contraseña para login biométrico futuro.
      _lastDni = documentoIdentidad;
      await UserPrefs.saveLastDni(documentoIdentidad);
      await UserPrefs.savePassword(password);

      _loading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _error = e.toString().replaceFirst('Exception: ', '');
      _loading = false;
      notifyListeners();
      return false;
    }
  }

  // ── Login biométrico ──────────────────────────────────────────────────

  /// Intenta autenticar usando biometría (huella / Face ID).
  ///
  /// Requiere que:
  /// 1. El dispositivo soporte biometría ([biometricAvailable]).
  /// 2. El usuario haya activado la opción ([biometricEnabled]).
  /// 3. Haya un DNI y contraseña previamente guardados ([lastDni]).
  ///
  /// Flujo: prompt biométrico nativo → login con credenciales cacheadas.
  Future<bool> loginWithBiometric() async {
    if (!canUseBiometric || _lastDni == null || _lastDni!.isEmpty) return false;

    final cachedPassword = await UserPrefs.getPassword();
    if (cachedPassword == null || cachedPassword.isEmpty) {
      _error = 'No hay credenciales guardadas. Inicie sesión con contraseña '
          'una vez y habilite la biometría desde configuración.';
      notifyListeners();
      return false;
    }

    _loading = true;
    _error = null;
    notifyListeners();

    try {
      // 1. Prompt biométrico nativo.
      final autenticado = await _biometricService.authenticate(
        message: 'Use su huella o Face ID para acceder',
      );
      if (!autenticado) {
        _loading = false;
        notifyListeners();
        return false;
      }

      // 2. Verificar zona horaria.
      final hora = await _sistemaDataSource.obtenerHoraServidor();
      if (!ZonaHorariaUtil.esMismaZona(
          DateTime.now().timeZoneOffset, hora.offsetUtc)) {
        _error = 'Zona horaria del dispositivo no coincide con la del servidor. '
            'Ajuste la hora y reintente.';
        _loading = false;
        notifyListeners();
        return false;
      }
      _horaServidor = hora;

      // 3. Login con credenciales cacheadas.
      final response =
          await _authDataSource.login(_lastDni!, cachedPassword);
      _aplicarSesion(response);

      _loading = false;
      notifyListeners();
      return true;
    } catch (e) {
      _error = e.toString().replaceFirst('Exception: ', '');
      _loading = false;
      notifyListeners();
      return false;
    }
  }

  // ── Preferencia biométrica ────────────────────────────────────────────

  /// Activa o desactiva el login biométrico para el usuario.
  Future<void> toggleBiometric(bool enabled) async {
    _biometricEnabledByUser = enabled;
    await UserPrefs.setBiometricEnabled(enabled);
    if (!enabled) {
      await UserPrefs.clearPassword();
    }
    notifyListeners();
  }

  // ── Internos ──────────────────────────────────────────────────────────

  /// Renueva el token de acceso usando el refresh token. Devuelve true si la
  /// sesión se renovó con éxito.
  Future<bool> _renovarSesion() async {
    if (_refreshEnProceso) return true;
    _refreshEnProceso = true;
    try {
      final refreshToken = _apiClient.refreshToken;
      if (refreshToken == null || refreshToken.isEmpty) return false;

      final response = await _authDataSource.refresh(refreshToken);
      _aplicarSesion(response);
      return true;
    } catch (_) {
      return false;
    } finally {
      _refreshEnProceso = false;
    }
  }

  void _aplicarSesion(LoginResponseModel response) {
    _apiClient.setToken(response.token);
    _apiClient.setRefreshToken(response.refreshToken);
    _user = response;
    _programarCierreDeSesion(response.expiraEnMinutos);
  }

  /// Programa el cierre automático de la sesión cuando el token expire
  /// (12 horas por turno). Al cumplirse, se cierra la sesión y se vuelve a la
  /// pantalla de inicio de sesión para que el siguiente operador ingrese.
  void _programarCierreDeSesion(int expiraEnMinutos) {
    _sesionTimer?.cancel();
    final minutos =
        expiraEnMinutos > 0 ? expiraEnMinutos : const Duration(hours: 12).inMinutes;
    _sesionTimer = Timer(Duration(minutes: minutos), _manejarSesionExpirada);
  }

  void _manejarSesionExpirada() {
    logout();
    _navigatorKey?.currentState?.pushNamedAndRemoveUntil('/', (route) => false);
  }

  void logout() {
    _sesionTimer?.cancel();
    _sesionTimer = null;
    _apiClient.clearToken();
    _user = null;
    _horaServidor = null;
    _error = null;
    UserPrefs.clearPassword();
    notifyListeners();
  }
}
