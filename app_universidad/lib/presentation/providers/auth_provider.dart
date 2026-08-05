import 'package:flutter/material.dart';
import '../../core/network/api_client.dart';
import '../../core/utils/zona_horaria_util.dart';
import '../../data/datasources/auth_remote_data_source.dart';
import '../../data/datasources/sistema_remote_data_source.dart';
import '../../data/models/hora_servidor_model.dart';
import '../../data/models/login_response_model.dart';

class AuthProvider extends ChangeNotifier {
  final ApiClient _apiClient;
  final AuthRemoteDataSource _authDataSource;
  final SistemaRemoteDataSource _sistemaDataSource;
  final GlobalKey<NavigatorState>? _navigatorKey;

  LoginResponseModel? _user;
  bool _loading = false;
  String? _error;
  HoraServidorModel? _horaServidor;
  bool _refreshEnProceso = false;

  AuthProvider(
    this._apiClient,
    this._authDataSource,
    this._sistemaDataSource, {
    GlobalKey<NavigatorState>? navigatorKey,
  }) : _navigatorKey = navigatorKey {
    _apiClient.onRefreshRequested = _renovarSesion;
    _apiClient.onSessionExpired = _manejarSesionExpirada;
  }

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
  }

  void _manejarSesionExpirada() {
    logout();
    _navigatorKey?.currentState?.pushNamedAndRemoveUntil('/', (route) => false);
  }

  void logout() {
    _apiClient.clearToken();
    _user = null;
    _horaServidor = null;
    _error = null;
    notifyListeners();
  }
}
