class HoraServidorModel {
  final DateTime fechaHoraUtc;
  final DateTime fechaHoraLocal;
  final String offsetUtc;
  final String zonaHoraria;

  HoraServidorModel({
    required this.fechaHoraUtc,
    required this.fechaHoraLocal,
    required this.offsetUtc,
    required this.zonaHoraria,
  });

  factory HoraServidorModel.fromJson(Map<String, dynamic> json) {
    return HoraServidorModel(
      fechaHoraUtc:
          DateTime.tryParse(json['fechaHoraUtc'] ?? '') ?? DateTime.now().toUtc(),
      fechaHoraLocal:
          DateTime.tryParse(json['fechaHoraLocal'] ?? '') ?? DateTime.now(),
      offsetUtc: json['offsetUtc'] ?? 'Z',
      zonaHoraria: json['zonaHoraria'] ?? '',
    );
  }

  /// Hora del servidor en su propia zona horaria, lista para mostrar.
  ///
  /// No uses [fechaHoraLocal] directamente: la API la serializa con offset
  /// (`2026-08-25T11:18:00-04:00`) y `DateTime.parse` devuelve un `DateTime`
  /// **en UTC** cuando la cadena trae offset. Leer `.hour` sobre ese valor daba
  /// las 15 en vez de las 11 — cuatro horas de más en Bolivia.
  ///
  /// Aquí se reconstruye la hora del servidor sumando su offset declarado al
  /// instante UTC. Se hace así, y no con `.toLocal()`, porque `.toLocal()`
  /// usaría la zona del teléfono: coincidiría hoy, pero mostraría una hora
  /// distinta a la del registro si el dispositivo estuviera en otra zona.
  DateTime get fechaHoraServidor {
    final offset = _parsearOffset(offsetUtc);
    if (offset == null) return fechaHoraLocal.toLocal();
    return fechaHoraUtc.toUtc().add(offset);
  }

  /// Formato "HH:mm" de la hora del servidor.
  String get horaFormateada {
    final h = fechaHoraServidor;
    return '${h.hour.toString().padLeft(2, '0')}:${h.minute.toString().padLeft(2, '0')}';
  }

  /// Interpreta los formatos que produce `OffsetUtcUtil.Formatear`: `"Z"`,
  /// `"-04:00"`, `"+05:30"`.
  static Duration? _parsearOffset(String valor) {
    final v = valor.trim();
    if (v.isEmpty) return null;
    if (v.toUpperCase() == 'Z') return Duration.zero;

    final m = RegExp(r'^([+-])(\d{1,2}):(\d{2})$').firstMatch(v);
    if (m == null) return null;

    final signo = m.group(1) == '-' ? -1 : 1;
    return Duration(
      hours: signo * int.parse(m.group(2)!),
      minutes: signo * int.parse(m.group(3)!),
    );
  }
}
