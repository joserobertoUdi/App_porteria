import '../entities/registro.dart';
import '../repositories/registro_repository.dart';

class BuscarRegistroUseCase {
  // aqui estoy usanod inyeccion de dependencias
  // el caso de uso no crea un repositorio, lo "recibe" por el constructor
  final RegistroRepository repository;

  BuscarRegistroUseCase(this.repository);
  // el metodo 'call' permite ejecutar la clase como si fuera una funcion 
  Future<Registro?> call(String ci) async {
    // aqui simplemente delegamos la tarea al repositorio
    if(ci.isEmpty) {
      return null; // Si el CI está vacío, retornamos null
    }
    return await repository.buscarRegistroPorCI(ci);
  }
}