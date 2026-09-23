import 'package:flutter_test/flutter_test.dart';
import 'package:app_universidad/data/models/login_response_model.dart';

void main() {
  group('LoginResponseModel.fromJson', () {
    test('parsea access token y refresh token', () {
      final json = {
        'token': 'jwt-access',
        'refreshToken': 'rt-123',
        'expiraEnMinutos': 180,
        'nombreCompleto': 'Juan Perez',
        'documentoIdentidad': '12345678',
        'tipoUsuarioId': 1,
        'rolId': 1,
        'rolNombre': 'Administrador',
        'fotoUrl': null,
      };

      final model = LoginResponseModel.fromJson(json);

      expect(model.token, 'jwt-access');
      expect(model.refreshToken, 'rt-123');
      expect(model.rolNombre, 'Administrador');
    });

    test('usa vacío si faltan los tokens', () {
      final model = LoginResponseModel.fromJson({'token': '', 'refreshToken': ''});

      expect(model.token, '');
      expect(model.refreshToken, '');
      expect(model.rolNombre, '');
    });
  });
}
