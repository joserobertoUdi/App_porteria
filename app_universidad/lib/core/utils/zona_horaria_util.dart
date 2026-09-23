/// Utilidades para validar que el dispositivo móvil comparte la zona horaria
/// del servidor. El servidor expone su offset UTC (ej: "-04:00" o "Z").
class ZonaHorariaUtil {
  ZonaHorariaUtil._();

  /// Compara el offset UTC del dispositivo (DateTime.now().timeZoneOffset)
  /// con el offset UTC del servidor (formato "+HH:mm"/"-HH:mm" o "Z").
  static bool esMismaZona(Duration dispositivo, String offsetUtcServidor) {
    final offsetServidor = _parsearOffset(offsetUtcServidor);
    if (offsetServidor == null) return false;
    return dispositivo.inMinutes == offsetServidor.inMinutes;
  }

  /// "Z" | "z" | "UTC" => 0; "+05:30" => 5h30m; "-04:00" => -4h.
  /// Devuelve null si el formato es inválido.
  static Duration? _parsearOffset(String offset) {
    final texto = offset.trim();
    if (texto.isEmpty) return null;

    if (texto == 'Z' || texto == 'z' || texto == 'UTC') {
      return Duration.zero;
    }

    final signo = texto.startsWith('-') ? -1 : 1;
    final partes = texto.replaceFirst(RegExp(r'^[+-]'), '').split(':');
    final horas = int.tryParse(partes[0]);
    if (horas == null) return null;

    var minutos = 0;
    if (partes.length > 1) {
      final m = int.tryParse(partes[1]);
      if (m == null) return null;
      minutos = m;
    }

    return Duration(hours: horas, minutes: minutos) * signo;
  }
}
