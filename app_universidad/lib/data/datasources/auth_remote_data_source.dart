import '../../core/network/api_client.dart';
import '../models/login_response_model.dart';

class AuthRemoteDataSource {
  final ApiClient _client;

  AuthRemoteDataSource(this._client);

  Future<LoginResponseModel> login(String documentoIdentidad, String password) async {
    final response = await _client.post('/api/Auth/login', {
      'documentoIdentidad': documentoIdentidad,
      'password': password,
    });
    return LoginResponseModel.fromJson(response);
  }

  /// Renueva la sesión usando el refresh token. Devuelve los nuevos tokens
  /// (access token + refresh token rotado).
  Future<LoginResponseModel> refresh(String refreshToken) async {
    final response = await _client.post('/api/Auth/refresh', {
      'refreshToken': refreshToken,
    });
    return LoginResponseModel.fromJson(response);
  }
}
