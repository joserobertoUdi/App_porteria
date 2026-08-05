import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:app_universidad/core/network/api_client.dart';

void main() {
  group('ApiClient - renovación de token (401)', () {
    test('ante 401 renueva el token y reintenta la petición', () async {
      var llamadasLista = 0;
      final mock = MockClient((request) async {
        if (request.url.path == '/api/Data/list') {
          llamadasLista++;
          if (llamadasLista == 1) {
            return http.Response('{"error":"token expirado"}', 401);
          }
          return http.Response('{"data":[1,2,3]}', 200);
        }
        if (request.url.path == '/api/Auth/refresh') {
          return http.Response(
              jsonEncode({'token': 'nuevo-access', 'refreshToken': 'rt-nuevo'}),
              200);
        }
        return http.Response('{}', 404);
      });

      final client = ApiClient(httpClient: mock);
      client.setToken('access-viejo');
      client.setRefreshToken('rt-viejo');
      client.onRefreshRequested = () async {
        final resp = await client.post('/api/Auth/refresh', {'refreshToken': 'rt-viejo'});
        client.setToken(resp['token'] as String);
        client.setRefreshToken(resp['refreshToken'] as String);
        return true;
      };

      final lista = await client.getList('/api/Data/list');

      expect(lista, [1, 2, 3]);
      expect(llamadasLista, 2);
      expect(client.refreshToken, 'rt-nuevo');
    });

    test('si la renovación falla invoca onSessionExpired y lanza error', () async {
      final mock = MockClient((request) async {
        if (request.url.path == '/api/Data/list') {
          return http.Response('{"error":"no autorizado"}', 401);
        }
        return http.Response('{"error":"sesión expirada"}', 401);
      });

      final client = ApiClient(httpClient: mock);
      client.setToken('access-viejo');
      client.setRefreshToken('rt-viejo');
      var sesionExpirada = false;
      client.onRefreshRequested = () async {
        try {
          await client.post('/api/Auth/refresh', {'refreshToken': 'rt-viejo'});
          return false;
        } catch (_) {
          return false;
        }
      };
      client.onSessionExpired = () => sesionExpirada = true;

      await expectLater(client.getList('/api/Data/list'), throwsException);
      expect(sesionExpirada, isTrue);
    });

    test('sin refresh token ni callback invoca onSessionExpired', () async {
      final mock = MockClient((request) async => http.Response('{"error":"no"}', 401));
      final client = ApiClient(httpClient: mock);
      client.setToken('access');
      var sesionExpirada = false;
      client.onSessionExpired = () => sesionExpirada = true;

      await expectLater(client.get('/api/Usuario/buscar/1'), throwsException);
      expect(sesionExpirada, isTrue);
    });

    test('401 sin token (login fallido) no renueva ni invoca onSessionExpired', () async {
      var refreshes = 0;
      final mock = MockClient((request) async {
        if (request.url.path == '/api/Auth/login') {
          return http.Response('{"error":"Credenciales inválidas. Quedan 4 intentos."}', 401);
        }
        refreshes++;
        return http.Response('{}', 404);
      });

      final client = ApiClient(httpClient: mock);
      var sesionExpirada = false;
      client.onRefreshRequested = () async {
        refreshes++;
        return false;
      };
      client.onSessionExpired = () => sesionExpirada = true;

      await expectLater(
        client.post('/api/Auth/login', {'documentoIdentidad': 'x', 'password': 'y'}),
        throwsA(isA<Exception>()),
      );
      expect(refreshes, 0);
      expect(sesionExpirada, isFalse);
    });

    test('una respuesta 401 dentro de un refresh no entra en recursión', () async {
      var refreshes = 0;
      final mock = MockClient((request) async {
        if (request.url.path == '/api/Auth/refresh') {
          refreshes++;
          return http.Response('{"error":"refresh inválido"}', 401);
        }
        return http.Response('{"error":"no autorizado"}', 401);
      });

      final client = ApiClient(httpClient: mock);
      client.setToken('access');
      client.setRefreshToken('rt');
      var sesionExpirada = false;
      client.onRefreshRequested = () async {
        try {
          await client.post('/api/Auth/refresh', {'refreshToken': 'rt'});
          return false;
        } catch (_) {
          return false;
        }
      };
      client.onSessionExpired = () => sesionExpirada = true;

      await expectLater(client.post('/api/Data/entrada', {}), throwsException);
      expect(refreshes, 1);
      expect(sesionExpirada, isTrue);
    });
  });
}
