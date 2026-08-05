import 'package:flutter_test/flutter_test.dart';
import 'package:app_universidad/data/models/hora_servidor_model.dart';

void main() {
  group('HoraServidorModel.fromJson', () {
    test('parsea respuesta del servidor', () {
      final json = {
        'fechaHoraUtc': '2026-07-31T19:30:00.000Z',
        'fechaHoraLocal': '2026-07-31T15:30:00.000',
        'offsetUtc': '-04:00',
        'zonaHoraria': 'SA Pacific Standard Time',
      };

      final model = HoraServidorModel.fromJson(json);

      expect(model.fechaHoraUtc, DateTime.parse('2026-07-31T19:30:00.000Z'));
      expect(model.fechaHoraLocal, DateTime.parse('2026-07-31T15:30:00.000'));
      expect(model.offsetUtc, '-04:00');
      expect(model.zonaHoraria, 'SA Pacific Standard Time');
    });

    test('aplica valores por defecto si faltan campos', () {
      final model = HoraServidorModel.fromJson({});

      expect(model.fechaHoraUtc, isNotNull);
      expect(model.fechaHoraLocal, isNotNull);
      expect(model.offsetUtc, 'Z');
      expect(model.zonaHoraria, '');
    });
  });
}
