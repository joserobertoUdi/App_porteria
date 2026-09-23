import '../entities/registro.dart';

abstract class RegistroRepository {
  // buscar a un estudiante por su CI (carnet de identidad)
  // retorna un registro si lo encuentra, o null si no lo encuentra
  Future<Registro?> buscarRegistroPorCI(String ci);
  // guarda un nuevo registro (entrada) en la base de datos
  // retorna la entidad "Registro" con todos los datos, (nombre, puerta, motivo, etc.)
  Future<void> registrarEntrada(Registro registro);
  // actualizar o guardar el registro marcando la salida
  Future<void> registrarSalida(Registro registro);
  
}