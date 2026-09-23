import 'dart:async';
import 'dart:io';
import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api_constants.dart';
import '../constants/error_messages.dart';
import '../services/logger_service.dart';

/// Cliente HTTP centralizado para la API de ServiciosGenerales.
///
/// Maneja automáticamente:
/// - Token JWT en cabecera `Authorization`.
/// - Renovación de token en respuesta 401 (refresh flow).
/// - Excepciones tipadas con mensajes de usuario comprensibles.
///
/// Las excepciones lanzadas son subclases de [AppException]:
/// - [NetworkException] → problemas de conectividad.
/// - [ServerException] → errores 4xx/5xx del backend.
/// - [AuthException] → 401 no renovable.
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

  ApiClient({http.Client? httpClient})
      : _httpClient = httpClient ?? http.Client();

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

  Future<Map<String, dynamic>> post(
      String endpoint, Map<String, dynamic> body) async {
    final url = Uri.parse('${ApiConstants.baseUrl}$endpoint');
    final response = await _send(
        () => _httpClient.post(url, headers: _headers, body: jsonEncode(body)));
    return _handleResponse(response);
  }

  /// Envía un POST multipart (form-data) con campos y opcionalmente un archivo.
  ///
  /// Se usa para el registro de entrada con foto, donde la imagen viaja en el
  /// mismo request que los datos del formulario.
  Future<Map<String, dynamic>> postMultipart(
    String endpoint, {
    required Map<String, String> fields,
    File? file,
    String fileFieldName = 'file',
  }) async {
    final url = Uri.parse('${ApiConstants.baseUrl}$endpoint');
    final request = http.MultipartRequest('POST', url);

    // Cabeceras de autenticación.
    if (_token != null) {
      request.headers['Authorization'] = 'Bearer $_token';
    }

    request.fields.addAll(fields);

    if (file != null && await file.exists()) {
      final stream = http.ByteStream(file.openRead());
      final length = await file.length();
      final multipartFile = http.MultipartFile(
        fileFieldName,
        stream,
        length,
        filename: file.path.split('/').last,
      );
      request.files.add(multipartFile);
    }

    final streamed = await request.send().timeout(_timeout);

    // Mapear a http.Response para reutilizar _handleResponse / _send lógica.
    final response = await http.Response.fromStream(streamed);

    if (response.statusCode == 401 && !_refreshing && _token != null) {
      final renovado = await _tryRefresh();
      if (renovado) {
        final retry = http.MultipartRequest('POST', url);
        if (_token != null) retry.headers['Authorization'] = 'Bearer $_token';
        retry.fields.addAll(fields);
        if (file != null && await file.exists()) {
          final stream = http.ByteStream(file.openRead());
          final length = await file.length();
          retry.files.add(http.MultipartFile(fileFieldName, stream, length,
              filename: file.path.split('/').last));
        }
        final retryStreamed = await retry.send().timeout(_timeout);
        final retryResponse = await http.Response.fromStream(retryStreamed);
        return _handleResponse(retryResponse);
      }
    }

    return _handleResponse(response);
  }

  Future<Map<String, dynamic>> put(
      String endpoint, Map<String, dynamic> body) async {
    final url = Uri.parse('${ApiConstants.baseUrl}$endpoint');
    final response = await _send(
        () => _httpClient.put(url, headers: _headers, body: jsonEncode(body)));
    return _handleResponse(response);
  }

  /// Ejecuta la petición y, si responde 401, intenta renovar el token una vez
  /// y reintenta la petición original. Si aún no hay token (p.ej. un login
  /// fallido) no se intenta renovar: el 401 se devuelve tal cual.
  Future<http.Response> _send(
      Future<http.Response> Function() request) async {
    try {
      var response = await request().timeout(_timeout);

      if (response.statusCode == 401 && !_refreshing && _token != null) {
        final renovado = await _tryRefresh();
        if (renovado) {
          response = await request().timeout(_timeout);
        }
      }

      return response;
    } on SocketException {
      throw NetworkException(AppErrors.network.generic);
    } on TimeoutException {
      throw NetworkException(AppErrors.network.timeout);
    } on HttpException {
      throw NetworkException(AppErrors.network.refused);
    } on FormatException {
      throw ServerException(AppErrors.server.generic);
    }
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
    } catch (e) {
      LoggerService.error('Error al renovar sesión', e);
      onSessionExpired?.call();
      return false;
    } finally {
      _refreshing = false;
    }
  }

  /// Procesa la respuesta HTTP y lanza excepciones tipadas según el código
  /// de estado.
  Map<String, dynamic> _handleResponse(http.Response response) {
    final body = response.body.isNotEmpty
        ? jsonDecode(response.body)
        : <String, dynamic>{};

    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (body is List) {
        return {'data': body};
      }
      return body is Map<String, dynamic> ? body : {'data': body};
    }

    // ── Extraer mensaje de error del backend ──
    final error = body is Map
        ? (body['error'] ?? body['mensaje'] ?? '')
        : '';

    final message = error.toString().isNotEmpty
        ? error.toString()
        : AppErrors.server.fromStatus(response.statusCode);

    // ── Lanzar excepción tipada según categoría ──
    switch (response.statusCode) {
      case 401:
        throw AuthException(AppErrors.auth.sessionExpired);
      case 403:
        throw AuthException(AppErrors.auth.unauthorized);
      case 404:
        throw ServerException(AppErrors.server.notFound,
            statusCode: response.statusCode);
      case 429:
        throw ServerException(AppErrors.server.tooManyRequests,
            statusCode: response.statusCode);
      default:
        if (response.statusCode >= 500) {
          throw ServerException(AppErrors.server.internal,
              statusCode: response.statusCode);
        }
        throw ServerException(message, statusCode: response.statusCode);
    }
  }
}
