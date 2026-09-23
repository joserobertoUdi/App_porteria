import '../../domain/entities/registro.dart';

// El modelo "extiende" (hereda) de la entidad

class RegistroModel extends Registro{
  RegistroModel({
    required super.id,
    required super.nombreCompleto,
    required super.ci,
    required super.tipo,
    required super.puerta,
    required super.motivo,
    required super.fechaHora
  });

  //de JSON a Obejto (para cuando .net8 nos envie los datos de SQL server a flutter)
  factory RegistroModel.fromJson(Map<String, dynamic> json) {
    return RegistroModel(
      id: json['id']?.toString(), // Convertir a String si no es nulo,
      nombreCompleto: json['nombreCompleto']?? '',
      ci: json['ci']?? '',
      tipo: json['tipo']?? '',
      puerta: json['puerta']?? '',
      motivo: json['motivo']?? '',
      //convertimos el texto del servidor a un objeto DateTime real
      fechaHora: json['fechaHora'] !=null
          ? DateTime.parse(json['fechaHora'])
          : DateTime.now(), // Si no viene fecha, usamos la fecha actual
    );
  }

  // de objeto a JSON (para cuando flutter envie datos a .net8 para guardarlos en SQL server)
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'nombreCompleto': nombreCompleto,
      'ci': ci,
      'tipo': tipo,
      'puerta': puerta,
      'motivo': motivo,
      // Convertimos el objeto DateTime a un formato de texto que el servidor pueda entender
      'fechaHora': fechaHora.toIso8601String(),
    };
  }
}