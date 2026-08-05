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
}
