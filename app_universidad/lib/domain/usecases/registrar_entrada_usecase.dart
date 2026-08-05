import '../entities/registro.dart';
import '../repositories/registro_repository.dart';

class RegistrarEntradaUsecase {
  final RegistroRepository repository;

  RegistrarEntradaUsecase(this.repository);

  Future<void> call(Registro registro) async {
    // nos aseguramos de que el tipo sea netamente entrada, para evitar errores
    // antes de enviar el registro a la base de datos
    if(registro.tipo != 'Entrada') {
      throw Exception('El tipo de registro debe ser "entrada".');
    }
    // Delegamos la tarea de registrar la entrada al repositorio
    await repository.registrarEntrada(registro);
  }
}