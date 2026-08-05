import '../../domain/entities/registro.dart';
import '../../domain/repositories/registro_repository.dart';
import '../datasources/registro_remote_data_source.dart';
import '../models/registro_model.dart';

class RegistroRepositoryImpl implements RegistroRepository{
  final RegistroRemoteDataSource remoteDataSource;

  RegistroRepositoryImpl(this.remoteDataSource);

  @override
  Future<Registro?> buscarRegistroPorCI(String ci) async {
    //llama a la API
    final modelo = await remoteDataSource.buscarPorCI(ci);
    // returna la entidad pura (si el modelo no es nulo)
    return modelo;
  }
  @override
  Future<void> registrarEntrada(Registro registro) async {
    //convierte a la entidad pura a un modelo (data) para poder enviarlo a la API
    final modelo = RegistroModel(
      id: registro.id,
      nombreCompleto: registro.nombreCompleto, 
      ci: registro.ci, 
      tipo: registro.tipo, 
      puerta: registro.puerta, 
      motivo: registro.motivo, 
      fechaHora: registro.fechaHora
      );
      await remoteDataSource.registrarEntrada(modelo);
  }
  @override
  Future<void> registrarSalida(Registro registro) async {
    //convierte a la entidad pura a un modelo (data) para poder enviarlo a la API
    final modelo = RegistroModel(
      id: registro.id,
      nombreCompleto: registro.nombreCompleto, 
      ci: registro.ci, 
      tipo: registro.tipo, 
      puerta: registro.puerta, 
      motivo: registro.motivo, 
      fechaHora: registro.fechaHora
      );
      await remoteDataSource.registrarSalida(modelo);
  }
}