class ParqueoActivoModel {
  final int idRegistro;
  final String matricula;
  final String? marca;
  final String? modelo;
  final String? color;
  final String nombreCompleto;
  final String documentoIdentidad;
  final DateTime fechaIngreso;
  final String puertaAcceso;
  final String? observaciones;

  ParqueoActivoModel({
    required this.idRegistro,
    required this.matricula,
    this.marca,
    this.modelo,
    this.color,
    required this.nombreCompleto,
    required this.documentoIdentidad,
    required this.fechaIngreso,
    required this.puertaAcceso,
    this.observaciones,
  });

  factory ParqueoActivoModel.fromJson(Map<String, dynamic> json) {
    return ParqueoActivoModel(
      idRegistro: json['idRegistro'] ?? 0,
      matricula: json['matricula'] ?? '',
      marca: json['marca'],
      modelo: json['modelo'],
      color: json['color'],
      nombreCompleto: json['nombreCompleto'] ?? '',
      documentoIdentidad: json['documentoIdentidad'] ?? '',
      fechaIngreso: json['fechaIngreso'] != null
          ? DateTime.parse(json['fechaIngreso'])
          : DateTime.now(),
      puertaAcceso: json['puertaAcceso'] ?? '',
      observaciones: json['observaciones'],
    );
  }
}

class EntradaCompletaResultadoModel {
  final int id;
  final int usuarioId;
  final int vehiculoId;
  final bool usuarioCreado;
  final bool vehiculoCreado;
  final String mensaje;

  EntradaCompletaResultadoModel({
    required this.id,
    required this.usuarioId,
    required this.vehiculoId,
    required this.usuarioCreado,
    required this.vehiculoCreado,
    required this.mensaje,
  });

  factory EntradaCompletaResultadoModel.fromJson(Map<String, dynamic> json) {
    return EntradaCompletaResultadoModel(
      id: json['id'] ?? 0,
      usuarioId: json['usuarioId'] ?? 0,
      vehiculoId: json['vehiculoId'] ?? 0,
      usuarioCreado: json['usuarioCreado'] ?? false,
      vehiculoCreado: json['vehiculoCreado'] ?? false,
      mensaje: json['mensaje'] ?? '',
    );
  }
}

class UsuarioModel {
  final int id;
  final String nombreCompleto;
  final String documentoIdentidad;
  final int tipoUsuarioId;

  UsuarioModel({
    required this.id,
    required this.nombreCompleto,
    required this.documentoIdentidad,
    required this.tipoUsuarioId,
  });

  factory UsuarioModel.fromJson(Map<String, dynamic> json) {
    return UsuarioModel(
      id: json['id'] ?? 0,
      nombreCompleto: json['nombreCompleto'] ?? '',
      documentoIdentidad: json['documentoIdentidad'] ?? '',
      tipoUsuarioId: json['tipoUsuarioId'] ?? 0,
    );
  }
}
