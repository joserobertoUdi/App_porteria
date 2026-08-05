import 'dart:async';
import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api_constants.dart';

class ApiClient {
  static const Duration _timeout = Duration(seconds: 20);

  final http.Client _httpClient;
  String? _token;
  String? _refreshToken;
  bool _refreshing = false;

  /// Invocado ante un 401 para renovar el token (POST /api/Auth/refresh).
  /// Devuelve true si la renovación fue exitosa y se actualizaron los tokens.
  Future<bool> Function()? onRefreshRequested;

  /// Invocado cuando la sesión no puede renovarse (token inválido/expirado).
  void Function()? onSessionExpired;

  ApiClient({http.Client? httpClient}) : _httpClient = httpClient ?? http.Client();

  void setToken(String token) => _token = token;
  void setRefreshToken(String refreshToken) => _refreshToken = refreshToken;
  String? get refreshToken => _refreshToken;
  void clearToken() {
    _token = null;
    _refreshToken = null;
  }

  bool get hasToken => _token != null && _token!.isNotEmpty;

  Map<String, String> get _headers {
    final headers = <String, String>{
      'Content-Type': 'application/json',
    };
    if (_token != null) {
      headers['Authorization'] = 'Bearer $_token';
    }
    return headers;
  }

  Future<Map<String, dynamic>> get(String endpoint) async {
    final url = Uri.parse('${ApiConstants.baseUrl}$endpoint');
    final response = await _send(() => _httpClient.get(url, headers: _headers));
    return _handleResponse(response);
  }

  Future<List<dynamic>> getList(String endpoint) async {
    final url = Uri.parse('${ApiConstants.baseUrl}$endpoint');
    final response = await _send(() => _httpClient.get(url, headers: _headers));
    final body = _handleResponse(response);
    return (body['data'] as List<dynamic>?) ?? [];
  }

  Future<Map<String, dynamic>> post(String endpoint, Map<String, dynamic> body) async {
    final url = Uri.parse('${ApiConstants.baseUrl}$endpoint');
    final response = await _send(
        () => _httpClient.post(url, headers: _headers, body: jsonEncode(body)));
    return _handleResponse(response);
  }

  Future<Map<String, dynamic>> put(String endpoint, Map<String, dynamic> body) async {
    final url = Uri.parse('${ApiConstants.baseUrl}$endpoint');
    final response = await _send(
        () => _httpClient.put(url, headers: _headers, body: jsonEncode(body)));
    return _handleResponse(response);
  }

  /// Ejecuta la petición y, si responde 401, intenta renovar el token una vez
  /// y reintenta la petición original. Si aún no hay token (p.ej. un login
  /// fallido) no se intenta renovar: el 401 se devuelve tal cual.
  Future<http.Response> _send(Future<http.Response> Function() request) async {
    var response = await request().timeout(_timeout);

    if (response.statusCode == 401 && !_refreshing && _token != null) {
      final renovado = await _tryRefresh();
      if (renovado) {
        response = await request().timeout(_timeout);
      }
    }

    return response;
  }

  Future<bool> _tryRefresh() async {
    if (onRefreshRequested == null) {
      onSessionExpired?.call();
      return false;
    }

    _refreshing = true;
    try {
      final ok = await onRefreshRequested!();
      if (!ok) {
        onSessionExpired?.call();
      }
      return ok;
    } catch (_) {
      onSessionExpired?.call();
      return false;
    } finally {
      _refreshing = false;
    }
  }

  Map<String, dynamic> _handleResponse(http.Response response) {
    final body = response.body.isNotEmpty ? jsonDecode(response.body) : <String, dynamic>{};
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (body is List) {
        return {'data': body};
      }
      return body is Map<String, dynamic> ? body : {'data': body};
    }
    final error = body is Map ? (body['error'] ?? body['mensaje'] ?? 'Error desconocido') : 'Error ${response.statusCode}';
    throw Exception(error.toString());
  }
}
