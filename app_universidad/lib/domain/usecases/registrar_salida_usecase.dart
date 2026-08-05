import '../entities/registro.dart';
import '../repositories/registro_repository.dart';

class RegistrarSalidaUsecase {
  final RegistroRepository repository;

  RegistrarSalidaUsecase(this.repository);

  Future<void> call(Registro registro) async {
    // nos aseguramos de que el tipo sea netamente salida, para evitar errores
    // antes de enviar el registro a la base de datos
    if(registro.tipo != 'Salida') {
      throw Exception('El tipo de registro debe ser "salida".');
    }
    // Delegamos la tarea de registrar la salida al repositorio
    await repository.registrarSalida(registro);
  }
}