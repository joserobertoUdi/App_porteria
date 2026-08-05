import 'package:flutter_test/flutter_test.dart';
import 'package:app_universidad/core/utils/zona_horaria_util.dart';

void main() {
  group('ZonaHorariaUtil.esMismaZona', () {
    test('misma zona horaria: dispositivo y servidor en UTC-4', () {
      final dispositivo = const Duration(hours: -4);
      expect(ZonaHorariaUtil.esMismaZona(dispositivo, '-04:00'), isTrue);
    });

    test('distinta zona horaria: dispositivo UTC-4, servidor UTC-6', () {
      final dispositivo = const Duration(hours: -4);
      expect(ZonaHorariaUtil.esMismaZona(dispositivo, '-06:00'), isFalse);
    });

    test('zona horaria con minutos: UTC+5:30', () {
      final dispositivo = const Duration(hours: 5, minutes: 30);
      expect(ZonaHorariaUtil.esMismaZona(dispositivo, '+05:30'), isTrue);
    });

    test('formato Z equivale a offset 0', () {
      expect(ZonaHorariaUtil.esMismaZona(Duration.zero, 'Z'), isTrue);
      expect(ZonaHorariaUtil.esMismaZona(const Duration(hours: 1), 'Z'), isFalse);
    });

    test('formato inválido devuelve false', () {
      final dispositivo = const Duration(hours: -4);
      expect(ZonaHorariaUtil.esMismaZona(dispositivo, ''), isFalse);
      expect(ZonaHorariaUtil.esMismaZona(dispositivo, 'abc'), isFalse);
      expect(ZonaHorariaUtil.esMismaZona(dispositivo, '-04:xx'), isFalse);
    });

    test('signo incorrecto pero mismo valor absoluto no coincide', () {
      final dispositivo = const Duration(hours: 4);
      expect(ZonaHorariaUtil.esMismaZona(dispositivo, '-04:00'), isFalse);
    });
  });
}
