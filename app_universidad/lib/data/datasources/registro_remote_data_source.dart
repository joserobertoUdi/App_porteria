import '../models/registro_model.dart';

abstract class RegistroRemoteDataSource {
  Future<RegistroModel?> buscarPorCI(String ci);
  Future<void> registrarEntrada(RegistroModel registro);
  Future<void> registrarSalida(RegistroModel registro);
}

// esta es solo una simulacion cuando tenga la la api terminada voy a 
// crear otra clase llamada RegistroRemoteDataSourceApi y la cambiare sin romper nada

class RegistoRemoteDataSourceMock implements RegistroRemoteDataSource{
  @override
  Future<RegistroModel?> buscarPorCI(String ci) async {
    // simulamos la latencia de red de 1s
    await Future.delayed(const Duration(seconds: 1));
    // simulamos que la base de datos encuentra a este estudiantes
    if (ci == '12345678') {
      return RegistroModel(
        id: '1',
        nombreCompleto: 'Estudiante de prueba',
        ci: '12345678',
        tipo: 'entrada',
        puerta: 'Principal',
        motivo: 'Clases',
        fechaHora: DateTime.now(),
      );
    }
    return null; //no lo encontre
  } 

  @override
  Future<void> registrarEntrada(RegistroModel registro) async {
    await Future.delayed(const Duration(seconds: 1));
    // aqui en un futuro estara mi codigo http.post
    print('[mock api] Entrada guardada en el servidor para: ${registro.nombreCompleto}');
  }

  @override
  Future<void> registrarSalida(RegistroModel registro) async {
    await Future.delayed(const Duration(seconds: 1));
    // aqui en un futuro estara mi codigo http.post
    print('[mock api] Salida guardada en el servidor para: ${registro.nombreCompleto}');
  }
}
