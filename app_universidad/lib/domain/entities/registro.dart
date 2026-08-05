class Registro {
  final String? id;
  final String nombreCompleto;
  final String ci;
  final String tipo;
  final String puerta;
  final String motivo;
  final DateTime fechaHora;

  Registro({
    this.id,
    required this.nombreCompleto,
    required this.ci,
    required this.tipo,
    required this.puerta,
    required this.motivo,
    required this.fechaHora,
  });
}
